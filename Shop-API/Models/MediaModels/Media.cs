
using Shop_API.Models.ProductModels;

namespace Shop_API.Models.MediaModels
{
    public class Media
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string StorageKey { get; set; } = null!;

        public string OriginalFileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long Size { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public byte[] RowVersion { get; set; } = null!;

        public ICollection<Brand> BrandLogos { get; set; } = new List<Brand>();

        public ICollection<Category> CategoryImages { get; set; } = new List<Category>();

        public ICollection<ProductVariantImage> ProductVariantImages { get; set; } = new List<ProductVariantImage>();
    }
}
