namespace Shop_API.DTOs.UserDTOs
{
    public class UserListDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public bool IsActive { get; set; }

        public int Level { get; set; }

        public bool CanManage { get; set; }

        public List<UserRoleDto> Roles { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}