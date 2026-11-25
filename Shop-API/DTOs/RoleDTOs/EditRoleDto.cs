using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.RoleDTOs
{
    public class EditRoleDto
    {
        [StringLength(100, ErrorMessage = "عنوان نقش نباید بیشتر از ۱۰۰ کاراکتر باشد.")]
        public string? DisplayName { get; set; }

        [StringLength(300, ErrorMessage = "توضیحات نباید بیشتر از ۳۰۰ کاراکتر باشد.")]
        public string? Description { get; set; }

        [Range(1, 1000000, ErrorMessage = "سطح نقش باید حداقل ۱ باشد.")]
        public int? Level { get; set; }

        public List<string>? Permissions { get; set; }
    }
}