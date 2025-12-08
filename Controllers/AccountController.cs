using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Systeme_RH.Models;
using Systeme_RH.ViewModels;

namespace Systeme_RH.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ==========================================
        // LOGIN (GET) - Affiche le formulaire
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            // Si déjà connecté, on redirige vers l'accueil
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // ==========================================
        // LOGIN (POST) - Traite la connexion
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Vérifier si l'utilisateur existe et est actif
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EstActif)
                {
                    ModelState.AddModelError(string.Empty, "Ce compte a été désactivé. Contactez l'administrateur.");
                    return View(model);
                }

                // Tentative de connexion
                // lockoutOnFailure: true pour bloquer après plusieurs échecs (sécurité brute force)
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Compte verrouillé suite à trop de tentatives.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
            }
            return View(model);
        }

        // ==========================================
        // LOGOUT
        // ==========================================
        [HttpPost] // Toujours en POST pour sécurité
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // ==========================================
        // ACCÈS REFUSÉ (Si un employé tente d'aller sur une page Admin)
        // ==========================================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}