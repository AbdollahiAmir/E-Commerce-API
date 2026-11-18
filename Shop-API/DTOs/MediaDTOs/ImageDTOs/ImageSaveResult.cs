namespace Shop_API.DTOs.MediaDTOs.ImageDTOs
{
    public class ImageSaveResult
    {
        public string StorageKey { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long Size { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
