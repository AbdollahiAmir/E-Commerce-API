using Microsoft.AspNetCore.Mvc;
using Shop_API.Authorization;
using Shop_API.Constants;
using Shop_API.DTOs.CategoryDTOs;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IProductServices;

namespace Shop_API.Controllers.ProductControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [HasPermission(Permissions.Categories.Create)]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto request, CancellationToken cancellationToken)
        {
            var result = await _categoryService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        [HasPermission(Permissions.Categories.View)]
        public async Task<ActionResult<PagedResult<CategoryListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.GetPagedAsync(page, pageSize, search, cancellationToken);

            return Ok(result);
        }

        [HttpGet("tree")]
        [HasPermission(Permissions.Categories.View)]
        public async Task<ActionResult<IReadOnlyList<CategoryTreeDto>>> GetTree(CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetTreeAsync(cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [HasPermission(Permissions.Categories.View)]
        public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [HasPermission(Permissions.Categories.Edit)]
        public async Task<ActionResult<CategoryDto>> Edit(int id, [FromBody] EditCategoryDto request, CancellationToken cancellationToken)
        {
            var result = await _categoryService.EditAsync(id, request, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [HasPermission(Permissions.Categories.Delete)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _categoryService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}