namespace Shop_API.DTOs.AuthDTOs
{
    public class AuthResponseDto
    {
        public string TokenType { get; set; } = "Bearer";

        public string AccessToken { get; set; } = null!;

        public DateTime AccessTokenExpiresAt { get; set; }

        public string RefreshToken { get; set; } = null!;

        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}