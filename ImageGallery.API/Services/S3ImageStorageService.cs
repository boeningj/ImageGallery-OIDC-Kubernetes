using Amazon.S3;
using Amazon.S3.Model;

namespace ImageGallery.API.Services
{
    public class S3ImageStorageService : IImageStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;

        public S3ImageStorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Stream> GetImageAsync(string fileName)
        {
            var bucketName = _configuration["AWS:S3ImageBucket"] ?? throw new InvalidOperationException("AWS:S3ImageBucket configuration is missing.");

            var response = await _s3Client.GetObjectAsync(bucketName, fileName);

            return response.ResponseStream;
        }

        public async Task<string> SaveImageAsync(byte[] imageBytes)
        {
            var bucketName = _configuration["AWS:S3ImageBucket"] ?? throw new InvalidOperationException("AWS:S3ImageBucket configuration is missing.");
            var fileName = $"{Guid.NewGuid()}.jpg";
            using var memoryStream = new MemoryStream(imageBytes);

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fileName,
                InputStream = memoryStream,
                ContentType = "image/jpeg"
            };

            await _s3Client.PutObjectAsync(putRequest);

            return fileName;
        }

        public async Task DeleteImageAsync(string fileName)
        {
            var bucketName = _configuration["AWS:S3ImageBucket"] ?? throw new InvalidOperationException("AWS:S3ImageBucket configuration is missing.");

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
    }
}