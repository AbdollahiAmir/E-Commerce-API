using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.CategoryDTOs
{
    public class EditCategoryDto
    {
        [StringLength(150, ErrorMessage = "نام دسته‌بندی نباید بیشتر از ۱۵۰ کاراکتر باشد.")]
        public string? Name { get; set; }

        [StringLength(200, ErrorMessage = "نشانی (Slug) نباید بیشتر از ۲۰۰ کاراکتر باشد.")]
        public string? Slug { get; set; }

        [StringLength(1000, ErrorMessage = "توضیحات نباید بیشتر از ۱۰۰۰ کاراکتر باشد.")]
        public string? Description { get; set; }

        public Guid? ImageMediaId { get; set; }

        public bool RemoveImage { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "شناسه‌ی دسته‌بندی والد معتبر نیست.")]
        public int? ParentCategoryId { get; set; }

        public bool RemoveParent { get; set; }

        public bool? IsActive { get; set; }
    }
}
