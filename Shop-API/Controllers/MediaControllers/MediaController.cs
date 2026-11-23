using Microsoft.AspNetCore.Mvc;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IMediaServices;

namespace Shop_API.Controllers.MediaControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10485760)]
        public async Task<ActionResult<MediaUploadDto>> Upload([FromForm] MediaUploadRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _mediaService.UploadAsync(request.Image, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<MediaListDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _mediaService.GetPagedAsync(page, pageSize, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MediaUploadDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediaService.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("تصویر مورد نظر یافت نشد.");

            return Ok(result);
        }

        [HttpGet("{id:guid}/file")]
        public async Task<IActionResult> GetFile(Guid id, [FromQuery] bool thumbnail = false, CancellationToken cancellationToken = default)
        {
            var result = await _mediaService.OpenAsync(id, thumbnail, cancellationToken)
                ?? throw new NotFoundException("فایل تصویر یافت نشد.");

            Response.Headers.CacheControl = "public,max-age=86400";

            return File(result.Stream, result.ContentType);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _mediaService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }
}