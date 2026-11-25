namespace Shop_API.Constants
{
    public sealed record PermissionInfo(string Key, string DisplayName, string Group);

    public static class Permissions
    {
        public const string ClaimType = "permission";

        public static class Media
        {
            public const string View = "media.view";
            public const string Upload = "media.upload";
            public const string Delete = "media.delete";
        }

        public static class Brands
        {
            public const string View = "brands.view";
            public const string Create = "brands.create";
            public const string Edit = "brands.edit";
            public const string Delete = "brands.delete";
        }

        public static class Categories
        {
            public const string View = "categories.view";
            public const string Create = "categories.create";
            public const string Edit = "categories.edit";
            public const string Delete = "categories.delete";
        }

        public static class Attributes
        {
            public const string View = "attributes.view";
            public const string Create = "attributes.create";
            public const string Edit = "attributes.edit";
            public const string Delete = "attributes.delete";
        }

        public static class Products
        {
            public const string View = "products.view";
            public const string Create = "products.create";
            public const string Edit = "products.edit";
            public const string Delete = "products.delete";
        }

        public static class Variants
        {
            public const string View = "variants.view";
            public const string Create = "variants.create";
            public const string Edit = "variants.edit";
            public const string Delete = "variants.delete";
        }

        public static class Roles
        {
            public const string View = "roles.view";
            public const string Manage = "roles.manage";
        }

        public static class Users
        {
            public const string View = "users.view";
            public const string Manage = "users.manage";
        }

        public static readonly IReadOnlyList<PermissionInfo> All =
        [
            new(Media.View, "مشاهده‌ی رسانه‌ها", "رسانه"),
            new(Media.Upload, "آپلود رسانه", "رسانه"),
            new(Media.Delete, "حذف رسانه", "رسانه"),

            new(Brands.View, "مشاهده‌ی برندها", "برند"),
            new(Brands.Create, "ایجاد برند", "برند"),
            new(Brands.Edit, "ویرایش برند", "برند"),
            new(Brands.Delete, "حذف برند", "برند"),

            new(Categories.View, "مشاهده‌ی دسته‌بندی‌ها", "دسته‌بندی"),
            new(Categories.Create, "ایجاد دسته‌بندی", "دسته‌بندی"),
            new(Categories.Edit, "ویرایش دسته‌بندی", "دسته‌بندی"),
            new(Categories.Delete, "حذف دسته‌بندی", "دسته‌بندی"),

            new(Attributes.View, "مشاهده‌ی ویژگی‌ها", "ویژگی"),
            new(Attributes.Create, "ایجاد ویژگی", "ویژگی"),
            new(Attributes.Edit, "ویرایش ویژگی", "ویژگی"),
            new(Attributes.Delete, "حذف ویژگی", "ویژگی"),

            new(Products.View, "مشاهده‌ی محصولات", "محصول"),
            new(Products.Create, "ایجاد محصول", "محصول"),
            new(Products.Edit, "ویرایش محصول", "محصول"),
            new(Products.Delete, "حذف محصول", "محصول"),

            new(Variants.View, "مشاهده‌ی انواع محصول", "نوع محصول"),
            new(Variants.Create, "ایجاد نوع محصول", "نوع محصول"),
            new(Variants.Edit, "ویرایش نوع محصول و تصاویر آن", "نوع محصول"),
            new(Variants.Delete, "حذف نوع محصول", "نوع محصول"),

            new(Roles.View, "مشاهده‌ی نقش‌ها", "نقش‌ها"),
            new(Roles.Manage, "مدیریت نقش‌ها", "نقش‌ها"),

            new(Users.View, "مشاهده‌ی کاربران", "کاربران"),
            new(Users.Manage, "مدیریت کاربران", "کاربران")
        ];
    }
}