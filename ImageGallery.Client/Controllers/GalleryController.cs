using ImageGallery.Client.ViewModels;
using ImageGallery.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Text;
using System.Text.Json;
using ImageGallery.Client.Services;

namespace ImageGallery.Client.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GalleryController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IRuntimeMetadataService _runtimeMetadataService;

        public GalleryController(IHttpClientFactory httpClientFactory, ILogger<GalleryController> logger, IConfiguration configuration,
            IWebHostEnvironment environment, IRuntimeMetadataService runtimeMetadataService)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;
            _environment = environment;
            _runtimeMetadataService = runtimeMetadataService ?? throw new ArgumentNullException(nameof(runtimeMetadataService));
        }

        public async Task<IActionResult> Index()
        {
            if (_environment.IsDevelopment() && User.Identity?.IsAuthenticated == true)
            {
                await LogIdentityInformation();
            }

            ViewBag.ImageGalleryPublicRoot = _configuration["ImageGalleryPublicRoot"];
            
            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/images/");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            UpdateApiPodMetadata(response);

            response.EnsureSuccessStatusCode();

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var images = await JsonSerializer.DeserializeAsync<List<Image>>(responseStream);
                return View(new GalleryIndexViewModel(images ?? new List<Image>()));
            }
        }

        public async Task<IActionResult> GetImageFile(Guid id)
        {
            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/images/{id}/file");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            UpdateApiPodMetadata(response);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var stream = await response.Content.ReadAsStreamAsync();

            return File(stream, "image/jpeg");
        }

        public async Task<IActionResult> EditImage(Guid id)
        {

            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/images/{id}");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            UpdateApiPodMetadata(response);

            response.EnsureSuccessStatusCode();

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var deserializedImage = await JsonSerializer.DeserializeAsync<Image>(responseStream);

                if (deserializedImage == null)
                {
                    throw new Exception("Deserialized image must not be null.");
                }

                var editImageViewModel = new EditImageViewModel()
                {
                    Id = deserializedImage.Id,
                    Title = deserializedImage.Title
                };

                return View(editImageViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(EditImageViewModel editImageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // create an ImageForUpdate instance
            var imageForUpdate = new ImageForUpdate(editImageViewModel.Title);

            // serialize it
            var serializedImageForUpdate = JsonSerializer.Serialize(imageForUpdate);

            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/images/{editImageViewModel.Id}")
            {
                Content = new StringContent(serializedImageForUpdate, System.Text.Encoding.Unicode, "application/json")
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            UpdateApiPodMetadata(response);

            response.EnsureSuccessStatusCode();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteImage(Guid id)
        {
            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/images/{id}");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            UpdateApiPodMetadata(response);

            response.EnsureSuccessStatusCode();

            return RedirectToAction("Index");
        }

        //[Authorize(Roles = "PayingUser")]
        [Authorize(Policy = "UserCanAddImage")]
        public IActionResult AddImage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "PayingUser")]
        [Authorize(Policy = "UserCanAddImage")]
        public async Task<IActionResult> AddImage(AddImageViewModel addImageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // create an ImageForCreation instance
            ImageForCreation? imageForCreation = null;

            // take the first (only) file in the Files list
            var imageFile = addImageViewModel.Files.First();

            if (imageFile.Length > 0)
            {
                using (var fileStream = imageFile.OpenReadStream())
                using (var ms = new MemoryStream())
                {
                    fileStream.CopyTo(ms);
                    imageForCreation = new ImageForCreation(
                        addImageViewModel.Title, ms.ToArray());
                }
            }

            // serialize it
            var serializedImageForCreation = JsonSerializer.Serialize(imageForCreation);

            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/images")
            {
                Content = new StringContent(serializedImageForCreation, System.Text.Encoding.Unicode, "application/json")
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            UpdateApiPodMetadata(response);

            response.EnsureSuccessStatusCode();

            return RedirectToAction("Index");
        }

        public async Task LogIdentityInformation()
        {
            // Get the saved identity token
            var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            // Get the saved access token
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            // Get the refresh token
            var refreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var userClaimsStringBuilder = new StringBuilder();
            foreach (var claim in User.Claims)
            {
                userClaimsStringBuilder.AppendLine($"Claim type: {claim.Type} - Claim value: {claim.Value}");
            }

            // Log token & claims
            _logger.LogInformation($"Identity token & user claims: " + $"\n{identityToken} \n{userClaimsStringBuilder}");
            _logger.LogInformation($"Access token: " + $"\n{accessToken}");
            _logger.LogInformation($"Refresh token: " + $"\n{refreshToken}");
        }

        private void UpdateApiPodMetadata(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("X-Pod-Name", out var values))
            {
                _runtimeMetadataService.ApiPodName = values.FirstOrDefault();
            }
        }
    }
}
