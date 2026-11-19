namespace Shop_API.DTOs.MediaDTOs
{
    public class MediaListDto
    {
        public Guid Id { get; set; }

        public string OriginalFileName { get; set; } = null!;

        public long Size { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsUsed { get; set; }
    }
}
