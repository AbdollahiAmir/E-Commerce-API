using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.AuthDTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "شماره موبایل الزامی است.")]
        [StringLength(11, ErrorMessage = "شماره موبایل معتبر نیست.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "رمز عبور الزامی است.")]
        [StringLength(128, ErrorMessage = "رمز عبور نباید بیشتر از ۱۲۸ کاراکتر باشد.")]
        public string Password { get; set; } = null!;
    }
}