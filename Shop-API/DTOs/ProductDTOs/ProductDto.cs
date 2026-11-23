namespace Shop_API.DTOs.ProductDTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public int? BrandId { get; set; }

        public string? BrandName { get; set; }

        public List<ProductCategoryDto> Categories { get; set; } = [];

        public bool IsActive { get; set; }

        public int VariantCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
