namespace Shop_API.DTOs.CategoryDTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string? Description { get; set; }

        public Guid? ImageMediaId { get; set; }

        public int? ParentCategoryId { get; set; }

        public IReadOnlyList<CategoryBreadcrumbDto> Breadcrumb { get; set; } = [];

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
