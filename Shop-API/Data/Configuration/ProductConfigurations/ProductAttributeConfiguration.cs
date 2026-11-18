using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.ProductModels;

namespace Shop_API.Data.Configuration.ProductConfigurations
{
    public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
            builder.ToTable("ProductAttributes");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Code)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.IsActive)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.HasIndex(a => a.Code)
                .IsUnique();

            builder.Property(a => a.RowVersion)
                .IsRowVersion();
        }
    }
}