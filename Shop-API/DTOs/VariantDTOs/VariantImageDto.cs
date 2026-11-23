namespace Shop_API.DTOs.VariantDTOs
{
    public class VariantImageDto
    {
        public int Id { get; set; }

        public int VariantId { get; set; }

        public Guid MediaId { get; set; }

        public string? AltText { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsMain { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}