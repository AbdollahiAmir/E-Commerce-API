using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.ProductDTOs
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "نام محصول الزامی است.")]
        [StringLength(200, ErrorMessage = "نام محصول نباید بیشتر از ۲۰۰ کاراکتر باشد.")]
        public string Name { get; set; } = null!;

        [StringLength(250, ErrorMessage = "نشانی (Slug) نباید بیشتر از ۲۵۰ کاراکتر باشد.")]
        public string? Slug { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "شناسه‌ی برند معتبر نیست.")]
        public int? BrandId { get; set; }

        [Required(ErrorMessage = "حداقل یک دسته‌بندی باید انتخاب شود.")]
        [MinLength(1, ErrorMessage = "حداقل یک دسته‌بندی باید انتخاب شود.")]
        [MaxLength(10, ErrorMessage = "هر محصول حداکثر می‌تواند در ۱۰ دسته‌بندی باشد.")]
        public List<int> CategoryIds { get; set; } = [];

        public bool IsActive { get; set; }
    }
}
