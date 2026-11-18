namespace Shop_API.Models.ProductModels
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public bool IsActive { get; set; } = false;

        public int? BrandId { get; set; }

        public Brand? Brand { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public byte[] RowVersion { get; set; } = null!;

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        
    }
}