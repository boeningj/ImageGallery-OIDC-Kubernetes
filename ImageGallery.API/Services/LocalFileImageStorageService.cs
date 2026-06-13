using Microsoft.AspNetCore.Hosting;    

namespace ImageGallery.API.Services
{
    public class LocalFileImageStorageService : IImageStorageService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public LocalFileImageStorageService(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        public Task<Stream> GetImageAsync(string fileName)
        {
            var webRootPath = _hostingEnvironment.WebRootPath;

            var filePath = Path.Combine(webRootPath, "Images", fileName);

            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return Task.FromResult(stream);
        }

        public async Task<string> SaveImageAsync(byte[] imageBytes)
        {
            var webRootPath = _hostingEnvironment.WebRootPath;
            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(webRootPath, "Images", fileName);

            await File.WriteAllBytesAsync(filePath, imageBytes);

            return fileName;
        }

        public Task DeleteImageAsync(string fileName)
        {
            var webRootPath = _hostingEnvironment.WebRootPath;

            var filePath = Path.Combine(webRootPath, "Images", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}