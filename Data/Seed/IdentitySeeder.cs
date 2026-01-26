using JobPortal_ServerSide.Models;
using Microsoft.AspNetCore.Identity;

namespace JobPortal_ServerSide.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


            string[] roles = { "Admin", "Developer","Company" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 🔐 Admin account
            var adminEmail = "admin@jobportal.com";
            var adminPassword = "Admin123!";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
            if (!context.Skills.Any())
            {
                var skills = new List<Skill>
    {
        new Skill { Name = "C#" },
        new Skill { Name = "ASP.NET Core" },
        new Skill { Name = "JavaScript" },
        new Skill { Name = "React" },
        new Skill { Name = "SQL" },
        new Skill { Name = "Entity Framework" },
        new Skill { Name = "Docker" },
        new Skill { Name = "Git" }
    };

                context.Skills.AddRange(skills);
                await context.SaveChangesAsync();
            }
        }
    }
}
