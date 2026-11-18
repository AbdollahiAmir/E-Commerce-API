using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.ProductModels;

namespace Shop_API.Data.Configuration.ProductConfigurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants", table =>
            {
                table.HasCheckConstraint(
                    "CK_ProductVariants_Price",
                    "[Price] >= 0");

                table.HasCheckConstraint(
                    "CK_ProductVariants_StockQuantity",
                    "[StockQuantity] >= 0");
            });

            builder.HasKey(v => v.Id);

            builder.Property(v => v.SKU)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.Description)
                .HasMaxLength(2000);

            builder.Property(v => v.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(v => v.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(v => v.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(v => v.CreatedAt)
                .IsRequired();

            builder.HasIndex(v => v.SKU)
                .IsUnique();

            builder.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
