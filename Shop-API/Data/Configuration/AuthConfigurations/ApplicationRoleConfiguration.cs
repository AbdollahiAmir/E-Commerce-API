using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.AuthModels;

namespace Shop_API.Data.Configuration.AuthConfigurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.ToTable("Roles", table =>
            {
                table.HasCheckConstraint("CK_Roles_Level", "[Level] >= 0");
            });

            builder.Property(r => r.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Description)
                .HasMaxLength(300);

            builder.Property(r => r.Level)
                .IsRequired();

            builder.Property(r => r.IsSystem)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.CreatedAt)
                .IsRequired();
        }
    }
}
