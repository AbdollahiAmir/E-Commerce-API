namespace Shop_API.Models.ProductModels
{
    public class Category
    {
        // اطلاعات عمومی
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        // دسته بندی والد
        public int? ParentCategoryId { get; set; }

        public Category? ParentCategory { get; set; }

        // لیست دسته بندی های فرزند
        public ICollection<Category> ChildCategories { get; set; } = new List<Category>();

        public bool IsActive { get; set; } = true;

        // تاریخ ایجاد و آپدیت دسته بندی
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // لیست محصولات
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
