namespace Shop_API.DTOs.RoleDTOs
{
    public class RoleDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string? Description { get; set; }

        public int Level { get; set; }

        public bool IsSystem { get; set; }

        public int UserCount { get; set; }

        public bool CanManage { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> Permissions { get; set; } = new();
    }
}