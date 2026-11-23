using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.VariantDTOs
{
    public class EditVariantDto
    {
        [StringLength(100, ErrorMessage = "SKU نباید بیشتر از ۱۰۰ کاراکتر باشد.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9._-]*\z", ErrorMessage = "SKU فقط می‌تواند شامل حروف انگلیسی، عدد و نشانه‌های . _ - باشد و باید با حرف یا عدد شروع شود.")]
        public string? Sku { get; set; }

        [StringLength(2000, ErrorMessage = "توضیحات نباید بیشتر از ۲۰۰۰ کاراکتر باشد.")]
        public string? Description { get; set; }

        [Range(0d, 1000000000000d, ErrorMessage = "قیمت باید بین صفر و یک تریلیون باشد.")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "موجودی نمی‌تواند منفی باشد.")]
        public int? StockQuantity { get; set; }

        public bool? IsActive { get; set; }

        [MaxLength(10, ErrorMessage = "هر نوع محصول حداکثر می‌تواند ۱۰ ویژگی داشته باشد.")]
        public List<VariantAttributeInputDto>? Attributes { get; set; }
    }
}