namespace Shop_API.Models.ProductModels
{
    public class ProductAttribute
    {
        // اطلاعات عمومی
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        // تاریخ ایجاد ویژگی
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // مقادیری که به این ویژگی اختصاص یافتند
        public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();

        // محصولاتی که از این ویژگی استفاده میکنند
        public ICollection<ProductAttributeAssignment> ProductAssignments { get; set; } = new List<ProductAttributeAssignment>();
    }
}
