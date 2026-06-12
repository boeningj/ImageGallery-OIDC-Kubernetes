using ImageGallery.API.DbContexts;
using ImageGallery.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using ImageGallery.Authorization;
using ImageGallery.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Amazon;
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddDbContext<GalleryContext>(options =>
{
    //options.UseSqlite(builder.Configuration["ConnectionStrings:ImageGalleryDBConnectionString"]);
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ImageGalleryDBConnectionString"]);
});

// register the repository
builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();

var storageProvider = builder.Configuration["Storage:Provider"] ?? throw new InvalidOperationException("Storage:Provider configuration is missing.");

if (string.Equals(storageProvider, "S3", StringComparison.OrdinalIgnoreCase))
{
    var awsRegion = builder.Configuration["AWS:Region"] ?? throw new InvalidOperationException("AWS:Region configuration is missing.");

    builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(awsRegion)));
    builder.Services.AddScoped<IImageStorageService, S3ImageStorageService>();
}
else if (string.Equals(storageProvider, "Local", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IImageStorageService, LocalFileImageStorageService>();
}
else
{
    throw new InvalidOperationException($"Unsupported storage provider: '{storageProvider}'.");
}

// register AutoMapper-related services
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<GalleryContext>("db");

// NOTE:
// This API supports THREE authentication modes, controlled via:
//   TokenSettings:Mode (appsettings.json)
//
// 1. Reference (opaque tokens, e.g. "96B7E0...")
//    → Issued by IdentityServer with AccessTokenType.Reference
//    → MUST use AddOAuth2Introspection (API calls IDP to validate)
//
// 2. Jwt (self-contained tokens, e.g. "eyJ...")
//    → Issued by IdentityServer with AccessTokenType.Jwt
//    → MUST use AddJwtBearer with Authority + Audience (local validation)
//
// 3. UserJwt (local dev tokens via `dotnet user-jwts`)
//    → Uses default AddJwtBearer() configuration from appsettings
//    → NOT compatible with IdentityServer tokens
//
// IMPORTANT:
// The API configuration MUST match the token type issued by the IDP.
// Mismatches will result in 401 Unauthorized.
//
//   IDP AccessTokenType.Jwt       → Mode = "Jwt"
//   IDP AccessTokenType.Reference → Mode = "Reference"
//
// Switching modes requires updating BOTH:
//   1. TokenSettings:Mode (this API)
//   2. ImageGallery.IDP client configuration (ImageGallery.IDP -> Config.cs)
//    - Set AccessTokenType on the "imagegalleryclient"://
//        AccessTokenType = AccessTokenType.Reference  → Mode = "Reference"
//        AccessTokenType = AccessTokenType.Jwt        → Mode = "Jwt"
//    - This determines the format of the access token issued by the IDP.
//
// NOTE:
// No changes are required in ImageGallery.Client for switching token types.
// The client is agnostic to whether tokens are JWT or reference tokens.
//
// If TokenSettings:Mode is missing or invalid, the application will fail fast at startup.
var mode = builder.Configuration["TokenSettings:Mode"];
if (string.IsNullOrWhiteSpace(mode))
{
    throw new InvalidOperationException("TokenSettings:Mode is not configured. Expected values: Reference, Jwt, or UserJwt.");
}

var authority = builder.Configuration["InternalIdentity:Authority"] ?? builder.Configuration["Authentication:Authority"];
var requireHttps = builder.Configuration.GetValue<bool?>("InternalIdentity:RequireHttpsMetadata") ?? builder.Configuration.GetValue<bool>("Authentication:RequireHttpsMetadata");
var apiClientId = builder.Configuration["Authentication:ApiClientId"];
var apiClientSecret = builder.Configuration["Authentication:ApiClientSecret"];

if (string.IsNullOrWhiteSpace(authority))
{
    throw new InvalidOperationException("Authentication__Authority is required.");
}

if (mode == "Reference" &&
   (string.IsNullOrWhiteSpace(apiClientId) ||
    string.IsNullOrWhiteSpace(apiClientSecret)))
{
    throw new InvalidOperationException("Reference mode requires Authentication__ApiClientId and Authentication__ApiClientSecret.");
}

// NOTE:  Prior to .NET 8.0, this was defined in JWT security token handler
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

switch (mode)
{
    case "Reference":
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Bearer";
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
            .AddOAuth2Introspection(options =>
            {
                options.Authority = authority;
                options.ClientId = apiClientId;
                options.ClientSecret = apiClientSecret;
                options.IntrospectionEndpoint = $"{authority}/connect/introspect";             
                options.NameClaimType = "given_name";
                options.RoleClaimType = "role";
            });
        break;

    case "Jwt":
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = "imagegalleryapi";
                options.RequireHttpsMetadata = requireHttps;
                // check the token's type header to avoid JWT Confusion attacks - DO NOT let APIs accept arbitrary tokens
                options.TokenValidationParameters = new ()
                {
                    NameClaimType = "given_name",
                    RoleClaimType = "role",
                    ValidTypes = new[] { "at+jwt" }
                };
            });
        break;

    case "UserJwt":
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(); // uses appsettings Authentication:Schemes
        break;
    default:
        throw new InvalidOperationException($"Invalid TokenSettings:Mode value: '{mode}'. Expected: Reference, Jwt, or UserJwt.");
}

builder.Services.AddAuthorization(authorizationOptions =>
{
    authorizationOptions.AddPolicy("UserCanAddImage", AuthorizationPolicies.CanAddImage());
    authorizationOptions.AddPolicy("ClientApplicationCanWrite", policyBuilder =>
    {
       policyBuilder.RequireClaim("scope", "imagegalleryapi.write"); 
    });
    authorizationOptions.AddPolicy("MustOwnImage", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.AddRequirements(new MustOwnImageRequirement());
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = check => check.Name == "self" });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Name != "self" });

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GalleryContext>();
    dbContext.Database.Migrate();
}

app.Run();
