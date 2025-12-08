using Microsoft.AspNetCore.Identity;
using Systeme_RH.Models;

namespace Systeme_RH.Data
{
    public static class DbInitializer
    {
        // J'ai renommé 'Initialize' en 'SeedAdminAsync' pour corriger ton erreur
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Création des Rôles
            string[] roleNames = { "Admin", "RH", "Employe" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Création de l'Admin par défaut
            var adminEmail = "admin@rhsystem.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nom = "Admin",
                    Prenom = "Systeme",
                    EmailConfirmed = true,
                    EstActif = true, 
                    DateCreation = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}