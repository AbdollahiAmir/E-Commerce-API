using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.AttributeDTOs
{
    public class EditAttributeDto
    {
        [StringLength(100, ErrorMessage = "نام ویژگی نباید بیشتر از ۱۰۰ کاراکتر باشد.")]
        public string? Name { get; set; }

        public bool? IsActive { get; set; }
    }
}
