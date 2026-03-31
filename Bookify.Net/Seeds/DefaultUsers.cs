using Microsoft.AspNetCore.Identity;

namespace Bookify.Net.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> UserManager)
        {
            ApplicationUser admin = new()
            {
                UserName = "admin",
                Email = "admin@bookify.com",
                FullName = "Admin",
                EmailConfirmed = true,   // we dont need verify admin email

            };

            var user = await UserManager.FindByEmailAsync(admin.Email);  // Search By Email

            if (user is null)
            {
                await UserManager.CreateAsync(admin, "Aa@Admin123");

                await UserManager.AddToRoleAsync(admin, AppRoles.Admin);

                // await UserManager.AddToRolesAsync(admin, new Lis);

            }



        }


    }
}
