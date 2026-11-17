namespace Shop_API.Models.ProductModels
{
    public class ProductAttributeAssignment
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int ProductAttributeId { get; set; }

        public ProductAttribute ProductAttribute { get; set; } = null!;

        public int DisplayOrder { get; set; }
    }
}
