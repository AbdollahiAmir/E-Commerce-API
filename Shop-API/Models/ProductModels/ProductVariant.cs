namespace Shop_API.Models.ProductModels
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public string SKU { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public ICollection<VariantAttribute> AttributeValues { get; set; } = new List<VariantAttribute>();

        public ICollection<ProductVariantImage> Images { get; set; } = new List<ProductVariantImage>();
    }
}
