using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.VariantDTOs
{
    public class EditVariantImageDto
    {
        [StringLength(250, ErrorMessage = "متن جایگزین نباید بیشتر از ۲۵۰ کاراکتر باشد.")]
        public string? AltText { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "ترتیب نمایش نمی‌تواند منفی باشد.")]
        public int? DisplayOrder { get; set; }

        public bool? IsMain { get; set; }
    }
}