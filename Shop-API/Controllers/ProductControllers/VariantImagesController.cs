using Microsoft.AspNetCore.Mvc;
using Shop_API.DTOs.VariantDTOs;
using Shop_API.Logics.IServices.IProductServices;

namespace Shop_API.Controllers.ProductControllers
{
    [Route("api/variants/{variantId:int}/images")]
    [ApiController]
    public class VariantImagesController : ControllerBase
    {
        private readonly IVariantImageService _variantImageService;

        public VariantImagesController(IVariantImageService variantImageService)
        {
            _variantImageService = variantImageService;
        }

        [HttpPost]
        public async Task<ActionResult<VariantImageDto>> Add(int variantId, [FromBody] AddVariantImageDto request, CancellationToken cancellationToken)
        {
            var result = await _variantImageService.AddAsync(variantId, request, cancellationToken);

            return CreatedAtAction(nameof(GetAll), new { variantId }, result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VariantImageDto>>> GetAll(int variantId, CancellationToken cancellationToken)
        {
            var result = await _variantImageService.GetAllAsync(variantId, cancellationToken);

            return Ok(result);
        }

        [HttpPatch("{imageId:int}")]
        public async Task<ActionResult<VariantImageDto>> Edit(int variantId, int imageId, [FromBody] EditVariantImageDto request, CancellationToken cancellationToken)
        {
            var result = await _variantImageService.EditAsync(variantId, imageId, request, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{imageId:int}")]
        public async Task<IActionResult> Delete(int variantId, int imageId, CancellationToken cancellationToken)
        {
            await _variantImageService.DeleteAsync(variantId, imageId, cancellationToken);

            return NoContent();
        }
    }
}