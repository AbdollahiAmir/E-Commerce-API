namespace Shop_API.Models.ProductModels
{
    public class Product
    {
        // اطلاعات عمومی
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // برند محصول، هر محصول یک برند دارد، اما اختیاری است
        public int? BrandId { get; set; }

        public Brand? Brand { get; set; }

        // تاریخ ایجاد و آپدیت محصول
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // دسته بندی (many to many)، ویژگی های اختصاص یافته، تنوع ها
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<ProductAttributeAssignment> AttributeAssignments { get; set; } = new List<ProductAttributeAssignment>();

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
