
using Microsoft.EntityFrameworkCore;
using Shop_API.Models.MediaModels;
using Shop_API.Models.ProductModels;

namespace Shop_API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Media> Media => Set<Media>();


    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<VariantAttribute> VariantAttributes => Set<VariantAttribute>();


    public DbSet<ProductVariantImage> ProductVariantImages => Set<ProductVariantImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
