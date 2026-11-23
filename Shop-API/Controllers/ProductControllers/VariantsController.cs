using Microsoft.AspNetCore.Mvc;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.VariantDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IProductServices;

namespace Shop_API.Controllers.ProductControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariantsController : ControllerBase
    {
        private readonly IVariantService _variantService;

        public VariantsController(IVariantService variantService)
        {
            _variantService = variantService;
        }

        [HttpPost]
        public async Task<ActionResult<VariantDto>> Create([FromBody] CreateVariantDto request, CancellationToken cancellationToken)
        {
            var result = await _variantService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<VariantListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] int? productId = null, [FromQuery] bool? isActive = null, CancellationToken cancellationToken = default)
        {
            var result = await _variantService.GetPagedAsync(page, pageSize, search, productId, isActive, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VariantDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _variantService.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("نوع محصول مورد نظر یافت نشد.");

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<VariantDto>> Edit(int id, [FromBody] EditVariantDto request, CancellationToken cancellationToken)
        {
            var result = await _variantService.EditAsync(id, request, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _variantService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}