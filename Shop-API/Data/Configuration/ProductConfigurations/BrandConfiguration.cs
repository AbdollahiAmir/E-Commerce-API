
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.ProductModels;

namespace Shop_API.Data.Configuration.ProductConfigurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(b => b.Slug)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.Description)
                .HasMaxLength(1000);

            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            builder.HasIndex(b => b.Slug)
                .IsUnique();

            builder.HasOne(b => b.LogoMedia)
                .WithMany(m => m.BrandLogos)
                .HasForeignKey(b => b.LogoMediaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(b => b.RowVersion)
                .IsRowVersion();
        }
    }
}
