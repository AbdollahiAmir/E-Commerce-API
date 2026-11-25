using Microsoft.AspNetCore.Mvc;
using Shop_API.Authorization;
using Shop_API.Constants;
using Shop_API.DTOs.AttributeDTOs;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IProductServices;

namespace Shop_API.Controllers.ProductControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttributesController : ControllerBase
    {
        private readonly IAttributeService _attributeService;

        public AttributesController(IAttributeService attributeService)
        {
            _attributeService = attributeService;
        }

        [HttpPost]
        [HasPermission(Permissions.Attributes.Create)]
        public async Task<ActionResult<AttributeDto>> Create([FromBody] CreateAttributeDto request, CancellationToken cancellationToken)
        {
            var result = await _attributeService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        [HasPermission(Permissions.Attributes.View)]
        public async Task<ActionResult<PagedResult<AttributeListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
        {
            var result = await _attributeService.GetPagedAsync(page, pageSize, search, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [HasPermission(Permissions.Attributes.View)]
        public async Task<ActionResult<AttributeDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _attributeService.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [HasPermission(Permissions.Attributes.Edit)]
        public async Task<ActionResult<AttributeDto>> Edit(int id, [FromBody] EditAttributeDto request, CancellationToken cancellationToken)
        {
            var result = await _attributeService.EditAsync(id, request, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [HasPermission(Permissions.Attributes.Delete)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _attributeService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}