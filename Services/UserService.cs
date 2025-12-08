using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models;

namespace Systeme_RH.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context; // AJOUT : Pour vérifier les liens Employe
        
        public UserService(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context) // AJOUT
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context; // AJOUT
        }

        public async Task<bool> CreateUserForEmployeAsync(Employe employe, string motDePasseProvisoire)
        {
            // 1. Préparation de l'ApplicationUser Identity
            var user = new ApplicationUser
            {
                UserName = employe.EmailPersonnel,
                Email = employe.EmailPersonnel,
                Nom = employe.Nom,
                Prenom = employe.Prenom,
                EmployeId = employe.Id, // Lien bidirectionnel (optionnel)
                EstActif = true,
                DateCreation = DateTime.UtcNow,
                EmailConfirmed = true
            };

            // 2. Création dans la base Identity avec le mot de passe
            var result = await _userManager.CreateAsync(user, motDePasseProvisoire);

            if (result.Succeeded)
            {
                // 3. Assignation du rôle "Employe" par défaut
                if (!await _roleManager.RoleExistsAsync("Employe"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Employe"));
                }
                
                await _userManager.AddToRoleAsync(user, "Employe");

                // 4. IMPORTANT : Mettre à jour le lien dans la table Employes
                employe.ApplicationUserId = user.Id;
                _context.Employes.Update(employe);
                await _context.SaveChangesAsync();

                // 5. Simulation Envoi Email
                Console.WriteLine("-------------------------------------------------------------");
                Console.WriteLine($"[SIMULATION EMAIL] À : {employe.EmailPersonnel}");
                Console.WriteLine($"Sujet : Bienvenue ! Vos accès RH");
                Console.WriteLine($"Login : {employe.EmailPersonnel}");
                Console.WriteLine($"Mot de passe : {motDePasseProvisoire}");
                Console.WriteLine("-------------------------------------------------------------");

                return true;
            }

            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Erreur création User : {error.Description}");
            }

            return false;
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.EstActif = !user.EstActif;

            if (!user.EstActif)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<ApplicationUser?> GetByEmployeIdAsync(int employeId)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeId == employeId);
        }

        public async Task<bool> ResetPasswordAsync(string userId, string nouveauMotDePasse)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, nouveauMotDePasse);

            return result.Succeeded;
        }

        /// <summary>
        /// CORRECTION MAJEURE : Récupère les utilisateurs non liés à un employé
        /// On inclut les rôles "Employe" et "RH"
        /// </summary>
        public async Task<List<ApplicationUser>> GetUnlinkedUsersAsync()
        {
            // 1. Récupérer tous les users des rôles Employe + RH
            var usersEmploye = await _userManager.GetUsersInRoleAsync("Employe");
            var usersRH = await _userManager.GetUsersInRoleAsync("RH");
            
            // 2. Fusionner les deux listes (sans doublons)
            var allUsers = usersEmploye.Union(usersRH).ToList();

            // 3. Récupérer tous les ApplicationUserId déjà utilisés dans la table Employes
            var linkedUserIds = await _context.Employes
                .Where(e => e.ApplicationUserId != null)
                .Select(e => e.ApplicationUserId)
                .ToListAsync();

            // 4. Filtrer : On garde uniquement les users qui ne sont PAS dans linkedUserIds
            var unlinkedUsers = allUsers
                .Where(u => !linkedUserIds.Contains(u.Id))
                .OrderBy(u => u.Email)
                .ToList();

            return unlinkedUsers;
        }

        /// <summary>
        /// Lie un utilisateur existant à un employé
        /// </summary>
        public async Task<bool> LinkEmployeToUserAsync(string userId, int employeId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // 1. Mise à jour du lien bidirectionnel (optionnel côté User)
            user.EmployeId = employeId;
            user.EstActif = true;

            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded) return false;

            // 2. IMPORTANT : Mise à jour du lien principal dans la table Employes
            var employe = await _context.Employes.FindAsync(employeId);
            if (employe != null)
            {
                employe.ApplicationUserId = userId;
                _context.Employes.Update(employe);
                await _context.SaveChangesAsync();
            }

            // 3. S'assurer que le user a au moins le rôle Employe
            if (!await _userManager.IsInRoleAsync(user, "Employe") && 
                !await _userManager.IsInRoleAsync(user, "RH") &&
                !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Employe");
            }

            return true;
        }
    }
}