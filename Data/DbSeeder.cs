using Microsoft.AspNetCore.Identity;
using HomeownersAssociation.Models;

namespace HomeownersAssociation.Data
{
    public class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Get the required services
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Define roles
            string[] roleNames = { "Admin", "Homeowner", "Staff" };

            // Create roles if they don't exist
            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user if it doesn't exist
            var adminEmail = "admin@homeowners.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    Address = "Admin Office",
                    LotNumber = "N/A",
                    BlockNumber = "N/A",
                    RegistrationDate = DateTime.Now,
                    IsApproved = true,
                    UserType = UserType.Admin
                };

                // Create the admin user with password
                var result = await userManager.CreateAsync(admin, "Admin@123");

                if (result.Succeeded)
                {
                    // Add admin to Admin role
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}