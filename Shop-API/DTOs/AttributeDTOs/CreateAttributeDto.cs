using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.AttributeDTOs
{
    public class CreateAttributeDto
    {
        [Required(ErrorMessage = "نام ویژگی الزامی است.")]
        [StringLength(100, ErrorMessage = "نام ویژگی نباید بیشتر از ۱۰۰ کاراکتر باشد.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "کد ویژگی الزامی است.")]
        [StringLength(100, ErrorMessage = "کد ویژگی نباید بیشتر از ۱۰۰ کاراکتر باشد.")]
        [RegularExpression(@"^[a-z][a-z0-9_]*\z", ErrorMessage = "کد ویژگی فقط می‌تواند شامل حروف کوچک انگلیسی، عدد و زیرخط باشد و باید با حرف شروع شود.")]
        public string Code { get; set; } = null!;

        public bool IsColor { get; set; }

        public bool IsActive { get; set; }
    }
}
