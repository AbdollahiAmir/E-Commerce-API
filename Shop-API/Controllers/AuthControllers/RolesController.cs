using Microsoft.AspNetCore.Mvc;
using Shop_API.Authorization;
using Shop_API.Constants;
using Shop_API.DTOs.RoleDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.Helpers;
using Shop_API.Logics.IServices.IAuthServices;

namespace Shop_API.Controllers.AuthControllers
{
    [Route("api/roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        [HasPermission(Permissions.Roles.Manage)]
        public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto request, CancellationToken cancellationToken)
        {
            var result = await _roleService.CreateAsync(User.GetUserId(), request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        [HasPermission(Permissions.Roles.View)]
        public async Task<ActionResult<IReadOnlyList<RoleListDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _roleService.GetAllAsync(User.GetUserId(), cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [HasPermission(Permissions.Roles.View)]
        public async Task<ActionResult<RoleDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _roleService.GetByIdAsync(User.GetUserId(), id, cancellationToken)
                ?? throw new NotFoundException("نقش مورد نظر یافت نشد.");

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [HasPermission(Permissions.Roles.Manage)]
        public async Task<ActionResult<RoleDto>> Edit(int id, [FromBody] EditRoleDto request, CancellationToken cancellationToken)
        {
            var result = await _roleService.EditAsync(User.GetUserId(), id, request, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [HasPermission(Permissions.Roles.Manage)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _roleService.DeleteAsync(User.GetUserId(), id, cancellationToken);

            return NoContent();
        }
    }
}