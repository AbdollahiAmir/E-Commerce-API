namespace Shop_API.DTOs.CategoryDTOs
{
    public class CategoryTreeDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public Guid? ImageMediaId { get; set; }

        public bool IsActive { get; set; }

        public List<CategoryTreeDto> Children { get; set; } = [];
    }
}
