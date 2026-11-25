using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.UserDTOs
{
    public class EditUserDto
    {
        [StringLength(100, ErrorMessage = "نام و نام خانوادگی نباید بیشتر از ۱۰۰ کاراکتر باشد.")]
        public string? FullName { get; set; }

        public bool? IsActive { get; set; }
    }
}