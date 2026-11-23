using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.VariantDTOs
{
    public class VariantAttributeInputDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "شناسه‌ی ویژگی معتبر نیست.")]
        public int AttributeId { get; set; }

        [Required(ErrorMessage = "مقدار ویژگی الزامی است.")]
        [StringLength(150, ErrorMessage = "مقدار ویژگی نباید بیشتر از ۱۵۰ کاراکتر باشد.")]
        public string Value { get; set; } = null!;

        [RegularExpression(@"^#[0-9A-Fa-f]{6}\z", ErrorMessage = "کد رنگ باید به شکل #RRGGBB باشد (مثلاً #FF0000).")]
        public string? ColorCode { get; set; }
    }
}