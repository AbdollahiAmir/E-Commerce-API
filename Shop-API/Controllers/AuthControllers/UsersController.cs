using Microsoft.AspNetCore.Mvc;
using Shop_API.Authorization;
using Shop_API.Constants;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.UserDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.Helpers;
using Shop_API.Logics.IServices.IAuthServices;

namespace Shop_API.Controllers.AuthControllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [HasPermission(Permissions.Users.View)]
        public async Task<ActionResult<PagedResult<UserListDto>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? roleId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetPagedAsync(User.GetUserId(), page, pageSize, search, isActive, roleId, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [HasPermission(Permissions.Users.View)]
        public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _userService.GetByIdAsync(User.GetUserId(), id, cancellationToken)
                ?? throw new NotFoundException("کاربر مورد نظر یافت نشد.");

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [HasPermission(Permissions.Users.Manage)]
        public async Task<ActionResult<UserDto>> Edit(int id, [FromBody] EditUserDto request, CancellationToken cancellationToken)
        {
            var result = await _userService.EditAsync(User.GetUserId(), id, request, cancellationToken);

            return Ok(result);
        }

        [HttpPatch("{id:int}/roles")]
        [HasPermission(Permissions.Users.Manage)]
        public async Task<ActionResult<UserDto>> SetRoles(int id, [FromBody] EditUserRolesDto request, CancellationToken cancellationToken)
        {
            var result = await _userService.SetRolesAsync(User.GetUserId(), id, request, cancellationToken);

            return Ok(result);
        }
    }
}