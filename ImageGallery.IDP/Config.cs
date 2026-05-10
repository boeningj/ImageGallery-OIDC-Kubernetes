using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace ImageGallery.IDP;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        { 
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "Your role(s)", new [] {"role"}),
            new IdentityResource("country", "The country you're living in", new List<string>() { "country" })
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("imagegalleryapi", "Image Gallery API", new [] { "role", "country" })
            {
                Scopes = { "imagegalleryapi.fullaccess", "imagegalleryapi.read", "imagegalleryapi.write" },
                //Required for authentication calling the token introspection endpiont at IDP to validate reference tokens
                ApiSecrets = { new Secret("apisecret".Sha256()) } 
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("imagegalleryapi.fullaccess"),
            new ApiScope("imagegalleryapi.read"),
            new ApiScope("imagegalleryapi.write")
        };    

    public static IEnumerable<Client> GetClients(IConfiguration config)
    {
        var baseUrl = config["ClientUrls:BaseUrl"] ?? throw new InvalidOperationException("ClientUrls:BaseUrl is not configured.");
        var clientSecret = config["Authentication:ClientSecret"] ?? throw new InvalidOperationException("Authentication:ClientSecret is not configured.");

        return new[]
        {            
            new Client()
            {
                ClientName = "Image Gallery",
                ClientId = "imagegalleryclient",
                AllowedGrantTypes = GrantTypes.Code,
                
                AccessTokenType = AccessTokenType.Reference, //for using reference (opaque) tokens instead of jwt
                // NOTE:  The above ontrols the type of access token that's issued to the client.
                //
                // AccessTokenType.Reference → opaque token (e.g. "96B7E0...")
                //   → API must use OAuth2 introspection (Mode = "Reference")
                // AccessTokenType.Jwt → self-contained JWT (e.g. "eyJ...")
                //   → API must use AddJwtBearer (Mode = "Jwt")
                // IMPORTANT:
                // This MUST match the API's TokenSettings:Mode configuration.
                // Mismatch will result in 401 Unauthorized.
                // See ImageGallery.API Program.cs for matching TokenSettings:Mode configuration
                AllowOfflineAccess = true, //required for refresh tokens
                UpdateAccessTokenClaimsOnRefresh = true, //this will ensure that claims in the access token are updated on a refresh
                AccessTokenLifetime = 120, //default is 1 hour
                //AuthorizationCodeLifetime = 300, //defalut is 5 minutes
                //IdentityTokenLifetime = 300, //default is 5 minutes
                RedirectUris = { $"{baseUrl}/signin-oidc" },
                PostLogoutRedirectUris = { $"{baseUrl}/signout-callback-oidc" },
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "roles",
                    //"imagegalleryapi.fullaccess",
                    "imagegalleryapi.read",
                    "imagegalleryapi.write",
                    "country"
                },
                ClientSecrets = { new Secret(clientSecret.Sha256()) },
                RequireConsent = true                    
            }
        };
    }
}