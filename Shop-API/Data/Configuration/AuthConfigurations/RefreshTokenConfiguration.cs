using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop_API.Models.AuthModels;

namespace Shop_API.Data.Configuration.AuthConfigurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TokenHash)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.ExpiresAt)
                .IsRequired();

            builder.HasIndex(t => t.TokenHash)
                .IsUnique();

            builder.HasIndex(t => t.UserId);

            builder.HasIndex(t => t.ExpiresAt);

            builder.HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}