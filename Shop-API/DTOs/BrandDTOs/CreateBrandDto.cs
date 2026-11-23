using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.BrandDTOs
{
    public class CreateBrandDto
    {
        [Required(ErrorMessage = "نام برند الزامی است.")]
        [StringLength(150, ErrorMessage = "نام برند نباید بیشتر از ۱۵۰ کاراکتر باشد.")]
        public string Name { get; set; } = null!;

        [StringLength(200, ErrorMessage = "نشانی (Slug) نباید بیشتر از ۲۰۰ کاراکتر باشد.")]
        public string? Slug { get; set; }

        [StringLength(1000, ErrorMessage = "توضیحات نباید بیشتر از ۱۰۰۰ کاراکتر باشد.")]
        public string? Description { get; set; }

        public Guid? LogoMediaId { get; set; }

        public bool IsActive { get; set; }
    }
}
