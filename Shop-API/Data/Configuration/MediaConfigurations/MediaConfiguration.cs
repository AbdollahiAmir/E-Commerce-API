
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.MediaModels;

namespace Shop_API.Data.Configuration.MediaConfigurations
{
    public class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.ToTable("Media", table =>
            {
                table.HasCheckConstraint(
                    "CK_Media_Size",
                    "[Size] > 0");

                table.HasCheckConstraint(
                    "CK_Media_Width",
                    "[Width] IS NULL OR [Width] > 0");

                table.HasCheckConstraint(
                    "CK_Media_Height",
                    "[Height] IS NULL OR [Height] > 0");
            });

            builder.HasKey(m => m.Id);

            builder.Property(m => m.StorageKey)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(m => m.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Size)
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.HasIndex(m => m.StorageKey)
                .IsUnique();

            builder.Property(m => m.RowVersion)
                .IsRowVersion();
        }
    }
}
