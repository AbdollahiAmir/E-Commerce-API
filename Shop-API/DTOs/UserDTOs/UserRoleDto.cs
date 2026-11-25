namespace Shop_API.DTOs.UserDTOs
{
    public class UserRoleDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public int Level { get; set; }
    }
}