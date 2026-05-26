using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenterManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CenterManagement.Infrastructure.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // =========================
            // Roles
            // =========================

            string[] roles =
            {
                "Admin",
                "Instructor",
                "Student"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            // =========================
            // Admin User
            // =========================

            string adminEmail =
                "admin@center.com";

            var adminUser =
                await userManager.FindByEmailAsync(
                    adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    FullName = "System Admin",

                    UserName = adminEmail,

                    Email = adminEmail,

                    EmailConfirmed = true,

                    IsActive = true
                };

                var result =
                    await userManager.CreateAsync(
                        user,
                        "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        user,
                        "Admin");
                }
            }
        }
    }
}