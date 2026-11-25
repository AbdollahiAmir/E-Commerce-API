using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.RoleDTOs
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "نام نقش الزامی است.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "نام نقش باید بین ۲ تا ۵۰ کاراکتر باشد.")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9_-]*$", ErrorMessage = "نام نقش باید با حرف انگلیسی شروع شود و فقط شامل حروف انگلیسی، عدد، خط تیره و زیرخط باشد.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "عنوان نقش الزامی است.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "عنوان نقش باید بین ۲ تا ۱۰۰ کاراکتر باشد.")]
        public string DisplayName { get; set; } = null!;

        [StringLength(300, ErrorMessage = "توضیحات نباید بیشتر از ۳۰۰ کاراکتر باشد.")]
        public string? Description { get; set; }

        [Range(1, 1000000, ErrorMessage = "سطح نقش باید حداقل ۱ باشد.")]
        public int Level { get; set; }

        public List<string>? Permissions { get; set; }
    }
}