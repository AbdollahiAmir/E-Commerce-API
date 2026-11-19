using System.ComponentModel.DataAnnotations;

namespace Shop_API.DTOs.MediaDTOs
{
    public class MediaUploadRequestDto
    {
        [Required(ErrorMessage = "لطفاً یک تصویر انتخاب کنید.")]
        public IFormFile Image { get; set; } = null!;
    }
}