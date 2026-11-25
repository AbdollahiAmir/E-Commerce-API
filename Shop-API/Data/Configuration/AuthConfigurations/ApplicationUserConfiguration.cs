using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.AuthModels;

namespace Shop_API.Data.Configuration.AuthConfigurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users", table =>
            {
                table.HasCheckConstraint(
                    "CK_Users_PhoneNumber",
                    "[PhoneNumber] LIKE '09[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
            });

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .IsUnicode(false)
                .IsFixedLength()
                .HasMaxLength(11);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique();
        }
    }
}