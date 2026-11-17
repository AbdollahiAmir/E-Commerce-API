namespace Shop_API.Models.ProductModels
{
    public class ProductVariantAttributeValue
    {
        public int ProductVariantId { get; set; }

        public ProductVariant ProductVariant { get; set; } = null!;

        public int ProductAttributeValueId { get; set; }

        public ProductAttributeValue ProductAttributeValue { get; set; } = null!;
    }
}
