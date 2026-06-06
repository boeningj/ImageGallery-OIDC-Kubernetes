using ImageGallery.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using ImageGallery.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var authority = builder.Configuration["Authentication:Authority"];
var metadataAddress = builder.Configuration["Authentication:MetadataAddress"];
var disableCertValidation = builder.Configuration.GetValue<bool>("OIDC:DISABLECERTVALIDATION", false);
Console.WriteLine("=====================================");
Console.WriteLine($"OIDC__DISABLECERTVALIDATION raw: {builder.Configuration["OIDC:DISABLECERTVALIDATION"]}");
Console.WriteLine($"DisableCertValidation parsed: {disableCertValidation}");
Console.WriteLine("=====================================");
var clientId = builder.Configuration["Authentication:ClientId"];
var clientSecret = builder.Configuration["Authentication:ClientSecret"];
var apiRoot = builder.Configuration["ImageGalleryAPIRoot"];

var missing = new List<string>();

if (string.IsNullOrWhiteSpace(authority))
    missing.Add("Authentication__Authority");

if (string.IsNullOrWhiteSpace(clientId))
    missing.Add("Authentication__ClientId");

if (string.IsNullOrWhiteSpace(clientSecret))
    missing.Add("Authentication__ClientSecret");

if (string.IsNullOrWhiteSpace(apiRoot))
    missing.Add("ImageGalleryAPIRoot");

if (missing.Any())
{
    throw new InvalidOperationException(
        $"Missing required configuration: {string.Join(", ", missing)}");
}

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure => 
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);
    
builder.Services.AddImageGalleryDataProtection(builder.Configuration);

// NOTE:  Prior to .NET 8.0, this was defined in JWT security token handler
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Middleware that's used to get the Access Token to pass to our API as the Bearer Token on each request
builder.Services.AddAccessTokenManagement();

// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri(apiRoot!);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => CreateHttpHandler(disableCertValidation))
.AddUserAccessTokenHandler(); //This ensures our API client will pass the access token on each request to our API
//Moreover, the line above will automatically refresh our access token when it's about to expire or when it has expired.
//This gives us long-lived access out of the box.

//used to interact with our IDP to revoke tokens
builder.Services.AddHttpClient("IDPClient", client =>
{   
   client.BaseAddress = new Uri(authority!);
}).ConfigurePrimaryHttpMessageHandler(() => CreateHttpHandler(disableCertValidation));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;    
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.AccessDeniedPath = "/Authentication/AccessDenied";
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = authority;    
    if (!string.IsNullOrWhiteSpace(metadataAddress))
    {
        options.MetadataAddress = metadataAddress;
    }    
    options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Authentication:RequireHttpsMetadata");
    if (disableCertValidation)
    {
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }
    options.ClientId = clientId;
    options.ClientSecret = clientSecret;
    options.ResponseType = "code"; //The middleware automatically enables PKCE protection, which is required for code flow
    //options.Scope.Add("openid");
    //options.Scope.Add("profile");
    //options.CallbackPath = new PathString("signin-oidc");
    // SignedOutCallbackPath:  default = host:port/signout-callback-oidc.
    // Must match with the post logout redirect URI at IDP client config if you want to automatically return
    // to the application after logging out of IdentityServer.
    // To change, set SignedOutCallbackPath eg: SignedOutCallbackPath = "pathaftersignout"
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.ClaimActions.Remove("aud");
    options.ClaimActions.DeleteClaim("sid");
    options.ClaimActions.DeleteClaim("idp");
    options.Scope.Add("roles");
    //options.Scope.Add("imagegalleryapi.fullaccess");
    options.Scope.Add("imagegalleryapi.read");
    options.Scope.Add("imagegalleryapi.write");
    options.Scope.Add("country");
    options.Scope.Add("offline_access"); //required scope for refresh tokens
    options.ClaimActions.MapJsonKey("role", "role");
    options.ClaimActions.MapUniqueJsonKey("country", "country");
    options.TokenValidationParameters = new ()
    {
        NameClaimType = "given_name",
        RoleClaimType = "role",
    };

    options.Events = new OpenIdConnectEvents
    {
        OnMessageReceived = ctx =>
        {
            Console.WriteLine("➡️ OnMessageReceived");
            return Task.CompletedTask;
        },
        OnAuthorizationCodeReceived = ctx =>
        {
            Console.WriteLine("➡️ AuthorizationCodeReceived");
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            Console.WriteLine("✅ TOKEN VALIDATED");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine("❌ AUTH FAILED: " + ctx.Exception);
            return Task.CompletedTask;
        },
        OnRemoteFailure = ctx =>
        {
            Console.WriteLine("❌ REMOTE FAILURE: " + ctx.Failure);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(authorizationOptions =>
{
   authorizationOptions.AddPolicy("UserCanAddImage", AuthorizationPolicies.CanAddImage()); 
});

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};

if (app.Environment.IsEnvironment("Docker"))
{
    // DEV / Docker → allow all proxies
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
}
else
{
    // Production → lock this down (placeholder for now)
    forwardedHeadersOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.0.0.0"), 24));
}

app.UseForwardedHeaders(forwardedHeadersOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An error occurred.");
        });
    });
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

app.Run();

static HttpMessageHandler CreateHttpHandler(bool disableCertValidation)
{
    if (disableCertValidation)
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }

    return new HttpClientHandler();
}