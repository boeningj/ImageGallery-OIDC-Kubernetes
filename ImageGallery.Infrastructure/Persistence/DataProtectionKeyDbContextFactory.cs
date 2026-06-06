using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ImageGallery.Infrastructure.Persistence;

public class DataProtectionKeyDbContextFactory
    : IDesignTimeDbContextFactory<DataProtectionKeyDbContext>
{
    public DataProtectionKeyDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ImageGalleryDBConnectionString");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string not found.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<DataProtectionKeyDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new DataProtectionKeyDbContext(optionsBuilder.Options);
    }
}