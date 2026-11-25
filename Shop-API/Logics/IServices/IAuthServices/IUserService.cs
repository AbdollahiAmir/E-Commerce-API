using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.UserDTOs;

namespace Shop_API.Logics.IServices.IAuthServices
{
    public interface IUserService
    {
        Task<PagedResult<UserListDto>> GetPagedAsync(int actorId, int page, int pageSize, string? search, bool? isActive, int? roleId, CancellationToken cancellationToken = default);

        Task<UserDto?> GetByIdAsync(int actorId, int id, CancellationToken cancellationToken = default);

        Task<UserDto> EditAsync(int actorId, int id, EditUserDto dto, CancellationToken cancellationToken = default);

        Task<UserDto> SetRolesAsync(int actorId, int id, EditUserRolesDto dto, CancellationToken cancellationToken = default);
    }
}