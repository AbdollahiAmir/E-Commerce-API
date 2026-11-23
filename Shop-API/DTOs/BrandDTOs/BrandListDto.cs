namespace Shop_API.DTOs.BrandDTOs
{
    public class BrandListDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public Guid? LogoMediaId { get; set; }

        public bool IsActive { get; set; }

        public int ProductCount { get; set; }
    }
}
