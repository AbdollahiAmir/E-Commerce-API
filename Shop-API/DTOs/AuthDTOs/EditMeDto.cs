using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.AuthDTOs
{
    public class EditMeDto
    {
        [StringLength(100, ErrorMessage = "نام و نام خانوادگی نباید بیشتر از ۱۰۰ کاراکتر باشد.")]
        public string? FullName { get; set; }
    }
}