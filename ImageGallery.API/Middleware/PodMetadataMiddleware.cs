namespace ImageGallery.API.Middleware
{
    public class PodMetadataMiddleware
    {
        private const string PodHeaderName = "X-Pod-Name";

        private readonly RequestDelegate _next;
        private readonly string _podName;

        public PodMetadataMiddleware(RequestDelegate next)
        {
            _next = next;
            _podName = Environment.GetEnvironmentVariable("POD_NAME") ?? "unknown";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers[PodHeaderName] = _podName;
            await _next(context);
        }
    }
}