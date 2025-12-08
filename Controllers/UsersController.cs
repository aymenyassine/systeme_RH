using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Systeme_RH.Models;
using Systeme_RH.ViewModels;

namespace Systeme_RH.Controllers
{
    [Authorize(Roles = "Admin")] // Sécurisé : Seul l'admin accède à ce contrôleur
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ==========================================
        // 1. LISTE DES UTILISATEURS (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    NomComplet = $"{user.Prenom} {user.Nom}",
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "Aucun",
                    EstActif = user.EstActif
                });
            }

            return View(model);
        }

        // ==========================================
        // 2. DÉTAILS D'UN UTILISATEUR
        // ==========================================
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            // On réutilise le ViewModel d'édition ou on en crée un spécifique pour l'affichage
            var model = new UserListViewModel
            {
                Id = user.Id,
                NomComplet = $"{user.Prenom} {user.Nom}",
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Aucun",
                EstActif = user.EstActif
            };

            return View(model);
        }

        // ==========================================
        // 3. CRÉER UN UTILISATEUR (GET + POST)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    EstActif = true,
                    DateCreation = DateTime.UtcNow
                };

                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Le mot de passe est requis.");
                }
                else
                {
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(model.RoleSelectionne))
                        {
                            await _userManager.AddToRoleAsync(user, model.RoleSelectionne);
                        }
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View(model);
        }

        // ==========================================
        // 4. MODIFIER UN UTILISATEUR (GET + POST)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Nom = user.Nom,
                Prenom = user.Prenom,
                EstActif = user.EstActif,
                RoleSelectionne = roles.FirstOrDefault()
            };

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", model.RoleSelectionne);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                // Mise à jour des infos de base
                user.Email = model.Email;
                user.UserName = model.Email; // Souvent l'email est le username
                user.Nom = model.Nom;
                user.Prenom = model.Prenom;
                user.EstActif = model.EstActif;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Gestion du changement de rôle
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var currentRole = userRoles.FirstOrDefault();

                    if (model.RoleSelectionne != currentRole)
                    {
                        if (!string.IsNullOrEmpty(currentRole))
                            await _userManager.RemoveFromRoleAsync(user, currentRole);
                        
                        if (!string.IsNullOrEmpty(model.RoleSelectionne))
                            await _userManager.AddToRoleAsync(user, model.RoleSelectionne);
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", model.RoleSelectionne);
            return View(model);
        }

        // ==========================================
        // 5. SUPPRIMER UN UTILISATEUR (GET + POST)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user); // Passer l'objet ApplicationUser à la vue de confirmation
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Optionnel : Vérifier si c'est le dernier admin pour éviter de se bloquer soi-même
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. FONCTIONNALITÉ EXTRA : RESET PASSWORD
        // (L'admin doit pouvoir réinitialiser un mot de passe oublié)
        // ==========================================
        [HttpGet]
        public IActionResult ResetPassword(string id)
        {
            if (id == null) return NotFound();
            var model = new AdminResetPasswordViewModel { Id = id };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrEmpty(model.Id)) return NotFound();
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Le nouveau mot de passe est requis.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Message"] = "Mot de passe réinitialisé avec succès.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}