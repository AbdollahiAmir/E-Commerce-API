
using Shop_API.Models.MediaModels;

namespace Shop_API.Models.ProductModels
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid ImageMediaId { get; set; }

        public Media ImageMedia { get; set; } = null!;

        public int? ParentCategoryId { get; set; }

        public Category? ParentCategory { get; set; }

        public ICollection<Category> ChildCategories { get; set; } = new List<Category>();

        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public byte[] RowVersion { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
