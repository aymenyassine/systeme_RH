using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Controllers
{
    [Authorize] // Il faut être connecté pour voir le tableau de bord
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICongeService _congeService;
        private readonly ApplicationDbContext _context; // Pour les compteurs rapides

        public HomeController(UserManager<ApplicationUser> userManager, 
                              ICongeService congeService,
                              ApplicationDbContext context)
        {
            _userManager = userManager;
            _congeService = congeService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new HomeDashboardViewModel
            {
                NomComplet = $"{user.Prenom} {user.Nom}",
                EmployeId = user.EmployeId
            };

            // Récupérer le(s) rôle(s)
            var roles = await _userManager.GetRolesAsync(user);
            model.RolePrincipal = roles.FirstOrDefault() ?? "Employé";

            // 1. CHARGEMENT DES DONNÉES EMPLOYÉ (Pour tout le monde qui a une fiche employé)
            if (user.EmployeId.HasValue)
            {
                var employe = await _context.Employes.FindAsync(user.EmployeId.Value);
                if (employe != null)
                {
                    model.MonSoldeConges = (int)employe.SoldeConges; // Cast si c'est un double
                    
                    // On utilise le service existant ou une requête directe légère
                    model.MesDemandesEnAttente = await _context.DemandesConges
                        .CountAsync(c => c.EmployeId == user.EmployeId && c.Etat == Models.Enums.EtatDemandeConge.EnAttente);
                    
                    model.AUnContratActif = await _context.Contrats
                        .AnyAsync(c => c.EmployeId == user.EmployeId && c.EstActif);
                }
            }

            // 2. CHARGEMENT DES DONNÉES RH / ADMIN (Statistiques globales)
            if (User.IsInRole("Admin") || User.IsInRole("RH"))
            {
                // Grâce à votre CongeService qui a déjà une méthode GetStatistiquesAsync
                var statsConges = await _congeService.GetStatistiquesAsync();
                
                model.TotalDemandesCongesEnAttente = statsConges.ContainsKey("EnAttente") ? statsConges["EnAttente"] : 0;
                model.TotalEmployes = await _context.Employes.CountAsync();
                model.TotalContratsActifs = await _context.Contrats.CountAsync(c => c.EstActif);
                model.TotalPostes = await _context.Postes.CountAsync();
                model.TotalDepartements = await _context.Departements.CountAsync();
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
