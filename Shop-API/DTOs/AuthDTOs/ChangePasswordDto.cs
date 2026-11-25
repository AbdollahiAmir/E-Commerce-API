using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.AuthDTOs
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است.")]
        [StringLength(128, ErrorMessage = "رمز عبور نباید بیشتر از ۱۲۸ کاراکتر باشد.")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "رمز عبور جدید الزامی است.")]
        [StringLength(128, ErrorMessage = "رمز عبور نباید بیشتر از ۱۲۸ کاراکتر باشد.")]
        public string NewPassword { get; set; } = null!;
    }
}