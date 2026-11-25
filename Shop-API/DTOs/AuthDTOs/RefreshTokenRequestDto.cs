using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.AuthDTOs
{
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "توکن نوسازی الزامی است.")]
        [StringLength(200, ErrorMessage = "توکن نوسازی نامعتبر است.")]
        public string RefreshToken { get; set; } = null!;
    }
}