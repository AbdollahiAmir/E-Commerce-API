using Microsoft.AspNetCore.Identity;

namespace Shop_API.Models.AuthModels
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
