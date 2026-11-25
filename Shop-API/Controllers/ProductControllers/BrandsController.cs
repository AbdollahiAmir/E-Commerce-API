using Microsoft.AspNetCore.Mvc;
using Shop_API.Authorization;
using Shop_API.Constants;
using Shop_API.DTOs.BrandDTOs;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IProductServices;

namespace Shop_API.Controllers.ProductControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpPost]
        [HasPermission(Permissions.Brands.Create)]
        public async Task<ActionResult<BrandDto>> Create([FromBody] CreateBrandDto request, CancellationToken cancellationToken)
        {
            var result = await _brandService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        [HasPermission(Permissions.Brands.View)]
        public async Task<ActionResult<PagedResult<BrandListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
        {
            var result = await _brandService.GetPagedAsync(page, pageSize, search, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [HasPermission(Permissions.Brands.View)]
        public async Task<ActionResult<BrandDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _brandService.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("برند مورد نظر یافت نشد.");

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [HasPermission(Permissions.Brands.Edit)]
        public async Task<ActionResult<BrandDto>> Update(int id, [FromBody] EditBrandDto request, CancellationToken cancellationToken)
        {
            var result = await _brandService.EditAsync(id, request, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [HasPermission(Permissions.Brands.Delete)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _brandService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}