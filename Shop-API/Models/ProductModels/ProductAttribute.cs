namespace Shop_API.Models.ProductModels
{
    public class ProductAttribute
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public bool IsColor { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public byte[] RowVersion { get; set; } = null!;

        public ICollection<VariantAttribute> VariantAttributes { get; set; } = new HashSet<VariantAttribute>();
    }
}