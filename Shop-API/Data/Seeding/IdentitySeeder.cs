using Microsoft.AspNetCore.Identity;
using Shop_API.Constants;
using Shop_API.Models.AuthModels;

namespace Shop_API.Data.Seeding
{
    public static class IdentitySeeder
    {
        private static readonly (string Name, string DisplayName, string Description, int Level)[] SystemRoles =
        [
            (RoleNames.Admin, "مدیر کل", "دسترسی کامل به همه‌ی بخش‌ها", RoleNames.AdminLevel),
            (RoleNames.Customer, "مشتری", "نقش پیش‌فرض کاربران ثبت‌نام‌شده", RoleNames.CustomerLevel)
        ];

        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var role in SystemRoles)
            {
                if (await roleManager.RoleExistsAsync(role.Name))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role.Name,
                    DisplayName = role.DisplayName,
                    Description = role.Description,
                    Level = role.Level,
                    IsSystem = true
                });

                if (!result.Succeeded && !await roleManager.RoleExistsAsync(role.Name))
                {
                    throw new InvalidOperationException($"ساخت نقش سیستمی {role.Name} ناموفق بود.");
                }
            }
        }
    }
}