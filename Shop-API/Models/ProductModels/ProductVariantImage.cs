namespace Shop_API.Models.ProductModels
{
    public class ProductVariantImage
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public string? AltText { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsMain { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
