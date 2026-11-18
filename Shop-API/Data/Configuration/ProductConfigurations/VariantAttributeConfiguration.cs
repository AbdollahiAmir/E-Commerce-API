using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.ProductModels;

namespace Shop_API.Data.Configuration.ProductConfigurations
{
    public class VariantAttributeConfiguration : IEntityTypeConfiguration<VariantAttribute>
    {
        public void Configure(EntityTypeBuilder<VariantAttribute> builder)
        {
            builder.ToTable("VariantAttributes");

            builder.HasKey(va => new
            {
                va.ProductVariantId,
                va.ProductAttributeId
            });

            builder.Property(va => va.Value)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(va => va.ColorCode)
                .HasMaxLength(20);

            builder.HasOne(va => va.ProductVariant)
                .WithMany(v => v.AttributeValues)
                .HasForeignKey(va => va.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(va => va.ProductAttribute)
                .WithMany(a => a.VariantAttributes)
                .HasForeignKey(va => va.ProductAttributeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}