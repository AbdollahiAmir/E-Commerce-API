using Shop_API.DTOs.RoleDTOs;

namespace Shop_API.Logics.IServices.IAuthServices
{
    public interface IRoleService
    {
        Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(int actorId, CancellationToken cancellationToken = default);

        Task<RoleDto> CreateAsync(int actorId, CreateRoleDto dto, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RoleListDto>> GetAllAsync(int actorId, CancellationToken cancellationToken = default);

        Task<RoleDto?> GetByIdAsync(int actorId, int id, CancellationToken cancellationToken = default);

        Task<RoleDto> EditAsync(int actorId, int id, EditRoleDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int actorId, int id, CancellationToken cancellationToken = default);
    }
}