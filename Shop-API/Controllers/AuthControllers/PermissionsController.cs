using Microsoft.AspNetCore.Mvc;
using Shop_API.Authorization;
using Shop_API.Constants;
using Shop_API.DTOs.RoleDTOs;
using Shop_API.Logics.Helpers;
using Shop_API.Logics.IServices.IAuthServices;

namespace Shop_API.Controllers.AuthControllers
{
    [Route("api/permissions")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public PermissionsController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [HasPermission(Permissions.Roles.View)]
        public async Task<ActionResult<IReadOnlyList<PermissionDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _roleService.GetPermissionsAsync(User.GetUserId(), cancellationToken);

            return Ok(result);
        }
    }
}