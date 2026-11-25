namespace Shop_API.DTOs.RoleDTOs
{
    public class RoleListDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string? Description { get; set; }

        public int Level { get; set; }

        public bool IsSystem { get; set; }

        public int UserCount { get; set; }

        public int PermissionCount { get; set; }

        public bool CanManage { get; set; }
    }
}