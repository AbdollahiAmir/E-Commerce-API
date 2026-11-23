namespace Shop_API.DTOs.VariantDTOs
{
    public class VariantAttributeValueDto
    {
        public int AttributeId { get; set; }

        public string AttributeName { get; set; } = null!;

        public string AttributeCode { get; set; } = null!;

        public bool IsColor { get; set; }

        public string Value { get; set; } = null!;

        public string? ColorCode { get; set; }
    }
}