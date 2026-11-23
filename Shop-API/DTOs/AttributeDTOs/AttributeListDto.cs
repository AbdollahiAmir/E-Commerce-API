namespace Shop_API.DTOs.AttributeDTOs
{
    public class AttributeListDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;

        public bool IsColor { get; set; }

        public bool IsActive { get; set; }

        public int UsageCount { get; set; }
    }
}
