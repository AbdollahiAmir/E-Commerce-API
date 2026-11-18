namespace Shop_API.Models.ProductModels
{
    public class VariantAttribute
    {
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public int ProductAttributeId { get; set; }
        public ProductAttribute ProductAttribute { get; set; } = null!;

        public string Value { get; set; } = null!;

        public string? ColorCode { get; set; }

        public byte[] RowVersion { get; set; } = null!;
    }
}