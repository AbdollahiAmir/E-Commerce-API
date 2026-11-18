using Shop_API.Models.MediaModels;

namespace Shop_API.Models.ProductModels
{
    public class Brand
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? LogoMediaId { get; set; }

        public Media? LogoMedia { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public byte[] RowVersion { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}