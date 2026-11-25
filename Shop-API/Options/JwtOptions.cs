using System.ComponentModel.DataAnnotations;

namespace Shop_API.Options
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; set; } = null!;

        [Required]
        public string Audience { get; set; } = null!;

        [Required]
        [MinLength(32)]
        public string Key { get; set; } = null!;

        [Range(1, 1440)]
        public int AccessTokenMinutes { get; set; } = 15;

        [Range(1, 365)]
        public int RefreshTokenDays { get; set; } = 30;
    }
}
