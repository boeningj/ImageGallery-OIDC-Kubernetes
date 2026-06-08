using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ImageGallery.Infrastructure.Persistence;

public class DataProtectionKeyDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionKeyDbContext(DbContextOptions<DataProtectionKeyDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}