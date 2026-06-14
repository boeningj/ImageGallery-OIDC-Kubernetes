namespace ImageGallery.Client.Services
{
    public class RuntimeMetadataService : IRuntimeMetadataService
    {
        private readonly string _platform;

        public RuntimeMetadataService(IConfiguration configuration)
        {
            _platform = configuration["Runtime:Platform"] ?? throw new InvalidOperationException("Runtime:Platform configuration is missing.");
        }

        public bool IsKubernetes => string.Equals(_platform, "Kubernetes", StringComparison.OrdinalIgnoreCase);

        public string ClientPodName => Environment.GetEnvironmentVariable("POD_NAME") ?? "unknown";

        public string? ApiPodName { get; set; }
    }
}