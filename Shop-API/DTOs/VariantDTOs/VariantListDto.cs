namespace Shop_API.DTOs.VariantDTOs
{
    public class VariantListDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string Sku { get; set; } = null!;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; }
    }
}