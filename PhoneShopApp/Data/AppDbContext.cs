using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhoneShopApp.Models;

namespace PhoneShopApp.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Phone> Phones => Set<Phone>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kezdőadatok (Seed)
        modelBuilder.Entity<Brand>().HasData(
            new Brand { Id = 1, Name = "Apple" },
            new Brand { Id = 2, Name = "Samsung" },
            new Brand { Id = 3, Name = "Xiaomi" }
        );

        modelBuilder.Entity<Phone>().HasData(
            new Phone { Id = 1, ModelName = "iPhone 16 Pro", Price = 499990, StockQuantity = 12, BrandId = 1, Description = "256 GB, Titán fekete" },
            new Phone { Id = 2, ModelName = "Galaxy S25 Ultra", Price = 549990, StockQuantity = 8, BrandId = 2, Description = "512 GB, S-Pen támogatás" },
            new Phone { Id = 3, ModelName = "Redmi Note 14 Pro", Price = 139990, StockQuantity = 25, BrandId = 3, Description = "128 GB, Kiváló ár-érték arány" }
        );
    }
}
