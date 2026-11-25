using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.UserDTOs
{
    public class EditUserRolesDto
    {
        [Required(ErrorMessage = "فهرست نقش‌ها الزامی است.")]
        public List<int> RoleIds { get; set; } = null!;
    }
}