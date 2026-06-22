namespace ImageGallery.Client.Services
{
    public interface IRuntimeMetadataService
    {
        bool IsKubernetes { get; }

        string ClientPodName { get; }

        string? ApiPodName { get; set; }
    }
}