using ImageGallery.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImageGallery.Infrastructure.Extensions;

public static class DataProtectionExtensions
{
    public static IServiceCollection AddImageGalleryDataProtection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ImageGalleryDB");

        services.AddDbContext<DataProtectionKeyDbContext>(options => options.UseSqlServer(connectionString));

        services
            .AddDataProtection()
            .SetApplicationName("ImageGallery")
            .PersistKeysToDbContext<DataProtectionKeyDbContext>();

        return services;
    }
}