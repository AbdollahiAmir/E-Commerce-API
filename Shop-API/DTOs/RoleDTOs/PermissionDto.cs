namespace Shop_API.DTOs.RoleDTOs
{
    public class PermissionDto
    {
        public string Key { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string Group { get; set; } = null!;

        public bool CanGrant { get; set; }
    }
}