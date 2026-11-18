using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.ProductModels;

namespace Shop_API.Data.Configuration.ProductConfigurations
{
    public class ProductVariantImageConfiguration : IEntityTypeConfiguration<ProductVariantImage>
    {
        public void Configure(EntityTypeBuilder<ProductVariantImage> builder)
        {
            builder.ToTable("ProductVariantImages", table =>
            {
                table.HasCheckConstraint(
                    "CK_ProductVariantImages_DisplayOrder",
                    "[DisplayOrder] >= 0");
            });

            builder.HasKey(i => i.Id);

            builder.Property(i => i.MediaId)
                .IsRequired();

            builder.Property(i => i.AltText)
                .HasMaxLength(250);

            builder.Property(i => i.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(i => i.IsMain)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            builder.HasIndex(i => new
            {
                i.ProductVariantId,
                i.IsMain
            })
                .IsUnique()
                .HasFilter("[IsMain] = 1");

            builder.HasIndex(i => i.MediaId)
                .IsUnique();

            builder.HasOne(i => i.ProductVariant)
                .WithMany(v => v.Images)
                .HasForeignKey(i => i.ProductVariantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Media)
                .WithOne(m => m.ProductVariantImage)
                .HasForeignKey<ProductVariantImage>(i => i.MediaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(i => i.RowVersion)
                .IsRowVersion();
        }
    }
}