namespace Shop_API.DTOs.ProductDTOs
{
    public class ProductListDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public int? BrandId { get; set; }

        public string? BrandName { get; set; }

        public bool IsActive { get; set; }

        public int VariantCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
