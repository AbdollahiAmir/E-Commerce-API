namespace Shop_API.DTOs.ProductDTOs
{
    public class ProductCategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;
    }
}
