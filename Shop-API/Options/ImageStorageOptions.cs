namespace Shop_API.Options
{
    public class ImageStorageOptions
    {
        public const string SectionName = "ImageStorage";

        public string RootPath { get; set; } = null!;

        public long MaxUploadBytes { get; set; } = 5 * 1024 * 1024;

        public int MaxSourceSide { get; set; } = 10000;

        public long MaxSourcePixels { get; set; } = 20_000_000;

        public int MainMaxSide { get; set; } = 1600;

        public int ThumbnailMaxSide { get; set; } = 400;

        public int MainQuality { get; set; } = 82;

        public int ThumbnailQuality { get; set; } = 78;

        public int MaxConcurrentProcessing { get; set; } = 2;
    }
}
