using Microsoft.AspNetCore.Identity;

namespace Shop_API.Models.AuthModels
{
    public class ApplicationRole : IdentityRole<int>
    {
        public string DisplayName { get; set; } = null!;

        public string? Description { get; set; }

        public int Level { get; set; }

        public bool IsSystem { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
