
namespace ImageGallery.API.Services
{
    public interface IImageStorageService
    {
        Task<Stream> GetImageAsync(string fileName);
        
        Task<string> SaveImageAsync(byte[] imageBytes);

        Task DeleteImageAsync(string fileName);
    }
}