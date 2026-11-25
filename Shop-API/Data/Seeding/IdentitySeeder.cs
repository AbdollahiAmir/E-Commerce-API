using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop_API.Constants;
using Shop_API.Logics.Helpers;
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

        public static async Task SeedAsync(IServiceProvider services)
        {
            await SeedRolesAsync(services);

            await SyncPermissionsAsync(services);

            await SeedAdminUserAsync(services);
        }

        private static async Task SeedRolesAsync(IServiceProvider services)
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

        private static async Task SyncPermissionsAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();

            var adminRoleId = await context.Roles
                .Where(r => r.Name == RoleNames.Admin)
                .Select(r => r.Id)
                .FirstAsync();

            var catalog = Permissions.All.Select(p => p.Key).ToList();

            var existing = await context.RoleClaims
                .Where(c => c.RoleId == adminRoleId && c.ClaimType == Permissions.ClaimType)
                .Select(c => c.ClaimValue!)
                .ToListAsync();

            var missing = catalog.Except(existing).ToList();

            if (missing.Count > 0)
            {
                context.RoleClaims.AddRange(missing.Select(key => new IdentityRoleClaim<int>
                {
                    RoleId = adminRoleId,
                    ClaimType = Permissions.ClaimType,
                    ClaimValue = key
                }));

                await context.SaveChangesAsync();
            }

            await context.RoleClaims
                .Where(c => c.ClaimType == Permissions.ClaimType && !catalog.Contains(c.ClaimValue!))
                .ExecuteDeleteAsync();
        }

        private static async Task SeedAdminUserAsync(IServiceProvider services)
        {
            var section = services.GetRequiredService<IConfiguration>().GetSection("AdminSeed");

            var phoneInput = section["PhoneNumber"];

            var password = section["Password"];

            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

            if (string.IsNullOrWhiteSpace(phoneInput) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("تنظیمات AdminSeed کامل نیست و کاربر ادمین ساخته نمی‌شود.");

                return;
            }

            if (!PhoneNumberHelper.TryNormalize(phoneInput, out var phone))
            {
                throw new InvalidOperationException("شماره موبایل AdminSeed معتبر نیست.");
            }

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByNameAsync(phone);

            if (user == null)
            {
                var fullName = section["FullName"];

                user = new ApplicationUser
                {
                    UserName = phone,
                    PhoneNumber = phone,
                    PhoneNumberConfirmed = true,
                    FullName = string.IsNullOrWhiteSpace(fullName) ? "مدیر سیستم" : fullName.Trim()
                };

                var created = await userManager.CreateAsync(user, password);

                if (!created.Succeeded)
                {
                    throw new InvalidOperationException("ساخت کاربر ادمین ناموفق بود: " + string.Join(" ", created.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                var added = await userManager.AddToRoleAsync(user, RoleNames.Admin);

                if (!added.Succeeded)
                {
                    throw new InvalidOperationException("افزودن نقش ادمین به کاربر ناموفق بود: " + string.Join(" ", added.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}