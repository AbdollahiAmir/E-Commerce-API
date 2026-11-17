namespace Shop_API.Models.ProductModels
{
    public class ProductAttributeValue
    {
        public int Id { get; set; }

        public int ProductAttributeId { get; set; }

        public ProductAttribute ProductAttribute { get; set; } = null!;

        public string Value { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? ColorCode { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        // تنوع های که از این مقدار استفاده میکنند
        public ICollection<ProductVariantAttributeValue> VariantAssignments { get; set; } = new List<ProductVariantAttributeValue>();
    }
}
