
namespace ImageGallery.API.Services
{
    public interface IImageStorageService
    {
        Task<string> SaveImageAsync(byte[] imageBytes);

        Task DeleteImageAsync(string fileName);
    }
}