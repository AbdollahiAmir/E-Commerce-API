using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.AuthDTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "نام و نام خانوادگی باید بین ۲ تا ۱۰۰ کاراکتر باشد.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "شماره موبایل الزامی است.")]
        [StringLength(20, ErrorMessage = "شماره موبایل معتبر نیست.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "رمز عبور الزامی است.")]
        [StringLength(128, ErrorMessage = "رمز عبور نباید بیشتر از ۱۲۸ کاراکتر باشد.")]
        public string Password { get; set; } = null!;
    }
}