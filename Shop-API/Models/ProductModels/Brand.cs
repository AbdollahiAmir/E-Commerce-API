namespace Shop_API.Models.ProductModels
{
    public class Brand
    {
        // اطلاعات عمومی
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // تاریخ ایجاد و آپدیت برند
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // لیست محصولات یک برند
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
