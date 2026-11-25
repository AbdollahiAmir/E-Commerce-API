using Microsoft.AspNetCore.Mvc;
using Shop_API.Authorization;
using Shop_API.Constants;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.ProductDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IProductServices;

namespace Shop_API.Controllers.ProductControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [HasPermission(Permissions.Products.Create)]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto request, CancellationToken cancellationToken)
        {
            var result = await _productService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        [HasPermission(Permissions.Products.View)]
        public async Task<ActionResult<PagedResult<ProductListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] int? brandId = null, [FromQuery] int? categoryId = null, [FromQuery] bool? isActive = null, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetPagedAsync(page, pageSize, search, brandId, categoryId, isActive, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [HasPermission(Permissions.Products.View)]
        public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _productService.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [HasPermission(Permissions.Products.Edit)]
        public async Task<ActionResult<ProductDto>> Edit(int id, [FromBody] EditProductDto request, CancellationToken cancellationToken)
        {
            var result = await _productService.EditAsync(id, request, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [HasPermission(Permissions.Products.Delete)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _productService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}