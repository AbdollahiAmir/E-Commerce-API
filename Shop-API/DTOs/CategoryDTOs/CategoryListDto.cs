namespace Shop_API.DTOs.CategoryDTOs
{
    public class CategoryListDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public int? ParentCategoryId { get; set; }

        public Guid? ImageMediaId { get; set; }

        public bool IsActive { get; set; }

        public int ChildCount { get; set; }

        public int DirectProductCount { get; set; }
    }
}
