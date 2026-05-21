using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthenticationController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var client = _httpClientFactory.CreateClient("IDPClient");

            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync();
            if(discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var clientId = _configuration["Authentication:ClientId"] ?? throw new InvalidOperationException("ClientId not configured");
            var clientSecret = _configuration["Authentication:ClientSecret"] ?? throw new InvalidOperationException("ClientSecret not configured");

            var accessTokenRevocationResponse = await client.RevokeTokenAsync(new ()
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken)
                });

            if (accessTokenRevocationResponse.IsError)
            {
                throw new Exception(accessTokenRevocationResponse.Error);
            }

            var refreshTokenRevocationResponse = await client.RevokeTokenAsync(new()
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken)
                });

            if (refreshTokenRevocationResponse.IsError)
            {
                throw new Exception(refreshTokenRevocationResponse.Error);
            }

            // Clear the local cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirects to the IDP linked to scheme "OpenIdConnectDefaults.AuthenticationScheme" (oidc) so
            // it can clear its own session/cookie
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            return new EmptyResult(); 
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}