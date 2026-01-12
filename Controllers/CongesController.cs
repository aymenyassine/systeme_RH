using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Systeme_RH.Interfaces;
using Systeme_RH.Models; // Pour ApplicationUser
using Systeme_RH.Models.Enums;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Controllers
{
    [Authorize] // Nécessite d'être connecté
    public class CongesController : Controller
    {
        private readonly ICongeService _congeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CongesController(ICongeService congeService, UserManager<ApplicationUser> userManager)
        {
            _congeService = congeService;
            _userManager = userManager;
        }

        // ==========================================
        // 1. INDEX (Intelligent : RH vs Employé)
        // ==========================================
        public async Task<IActionResult> Index(EtatDemandeConge? statut, TypeConge? type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // CAS 1 : C'est un RH ou Admin -> Vue Globale avec Filtres
            if (User.IsInRole("RH") || User.IsInRole("Admin"))
            {
                var viewModel = await _congeService.GetAllAsync(statut, type, null);

                // On prépare les listes pour les filtres dans la vue
                ViewBag.IsRH = true;
                return View(viewModel);
            }

            // CAS 2 : C'est un Employé lambda -> Vue Personnelle
            // Si l'utilisateur n'est pas lié à une fiche employé, on gère l'erreur
            if (!user.EmployeId.HasValue)
            {
                return View("Error", new ErrorViewModel { RequestId = "Compte utilisateur non lié à une fiche Employé." });
            }

            var mesConges = await _congeService.GetByEmployeAsync(user.EmployeId.Value);

            // On enveloppe la liste dans le ViewModel global pour garder la même structure de Vue si possible
            // ou on utilise une vue dédiée. Ici, j'adapte pour le ViewModel global.
            var modelEmploye = new CongeListViewModel
            {
                Conges = mesConges,
                TotalDemandes = mesConges.Count,
                DemandesEnAttente = mesConges.Count(c => c.Statut == EtatDemandeConge.EnAttente),
                DemandesValidees = mesConges.Count(c => c.Statut == EtatDemandeConge.Approuve),
                DemandesRefusees = mesConges.Count(c => c.Statut == EtatDemandeConge.Refuse)
            };

            ViewBag.IsRH = false;
            return View(modelEmploye);
        }

        // ==========================================
        // 2. DETAILS
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var conge = await _congeService.GetByIdAsync(id);
            if (conge == null) return NotFound();

            // Sécurité : Un employé ne doit pas voir les congés d'un autre (sauf RH)
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("RH") && !User.IsInRole("Admin"))
            {
                if (user == null || conge.EmployeId != user.EmployeId)
                {
                    return Forbid();
                }
            }

            return View(conge);
        }

        // ==========================================
        // 3. CREATE (Demander un congé)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CongeViewModel { DateDebut = DateTime.Today, DateFin = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CongeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.EmployeId.HasValue)
            {
                ModelState.AddModelError("", "Votre compte utilisateur n'est associé à aucun profil Employé.");
                return View(model);
            }

            try
            {
                await _congeService.CreateDemandeAsync(model, user.EmployeId.Value);
                TempData["Success"] = "Votre demande de congé a été enregistrée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex) // On attrape les erreurs métier du Service (Solde, Dates...)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Une erreur technique est survenue.");
            }

            return View(model);
        }

        // ==========================================
        // 4. ANNULER (Action Employé)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.EmployeId.HasValue) return Forbid();

            try
            {
                bool result = await _congeService.CancelDemandeAsync(id, user.EmployeId.Value);
                if (result)
                {
                    TempData["Success"] = "Demande annulée.";
                }
                else
                {
                    TempData["Error"] = "Impossible d'annuler cette demande.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. VALIDER (Action RH)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "RH, Admin")] // Sécurité critique
        public async Task<IActionResult> Valider(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            // On suppose que le valideur a un ID employé, sinon mettre 0 ou gérer le cas
            int validateurId = (user != null && user.EmployeId.HasValue) ? user.EmployeId.Value : 0;

            try
            {

                bool success = await _congeService.ValiderDemandeAsync(id, validateurId);
                if (success)
                    TempData["Success"] = "Demande validée et solde mis à jour.";
                else
                    TempData["Error"] = "Demande introuvable.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = $"Erreur de validation : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. REFUSER (Action RH)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "RH, Admin")]
        public async Task<IActionResult> Refuser(int id, string motifRefus)
        {
            if (string.IsNullOrWhiteSpace(motifRefus))
            {
                TempData["Error"] = "Un motif de refus est obligatoire.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            int validateurId = (user != null && user.EmployeId.HasValue) ? user.EmployeId.Value : 0;
            try
            {
                bool success = await _congeService.RefuserDemandeAsync(id, validateurId, motifRefus);
                if (success)
                    TempData["Success"] = "Demande refusée.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 7. DASHBOARD (Pour les widgets RH)
        // ==========================================
        [Authorize(Roles = "RH, Admin")]
        public async Task<IActionResult> DashboardWidget()
        {
            // Cette méthode peut être appelée via AJAX ou ViewComponent pour afficher les stats
            var stats = await _congeService.GetStatistiquesAsync();
            return Json(stats);
        }

        // ==========================================
        // MESCONGES - Vue personnelle des congés de l'employé connecté
        // ==========================================
        public async Task<IActionResult> MesConges(EtatDemandeConge? statut = null, TypeConge? type = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Vérifier que l'utilisateur est lié à un employé
            if (!user.EmployeId.HasValue)
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = "Votre compte utilisateur n'est pas associé à un profil employé."
                });
            }

            // Récupérer les congés de l'employé avec filtres optionnels
            var mesConges = await _congeService.GetByEmployeAsync(user.EmployeId.Value);

            // Appliquer les filtres si spécifiés
            if (statut.HasValue)
            {
                mesConges = mesConges.Where(c => c.Statut == statut.Value).ToList();
            }

            if (type.HasValue)
            {
                mesConges = mesConges.Where(c => c.Type == type.Value).ToList();
            }

            // Préparer le ViewModel avec statistiques
            var viewModel = new CongeListViewModel
            {
                Conges = mesConges,
                StatutFilter = statut,
                TypeFilter = type,
                EmployeIdFilter = user.EmployeId.Value,
                TotalDemandes = mesConges.Count,
                DemandesEnAttente = mesConges.Count(c => c.Statut == EtatDemandeConge.EnAttente),
                DemandesValidees = mesConges.Count(c => c.Statut == EtatDemandeConge.Approuve),
                DemandesRefusees = mesConges.Count(c => c.Statut == EtatDemandeConge.Refuse)
            };

            // Récupérer le solde depuis le premier congé (qui contient déjà les infos employé)
            if (mesConges.Any())
            {
                ViewBag.SoldeConges = mesConges.First().EmployeSoldeConges;
                ViewBag.EmployeNom = mesConges.First().EmployeNom;
            }
            else
            {
                // Si pas de congés, on peut mettre des valeurs par défaut
                ViewBag.SoldeConges = 0;
                ViewBag.EmployeNom = user.UserName;
            }

            ViewBag.IsEmployeView = true; // Pour différencier dans la vue

            return View(viewModel);
        }

        [Authorize(Roles = "RH, Admin")]
        public async Task<IActionResult> Rapports()
        {
            var rapport = await _congeService.GetRapportAsync();
            return View(rapport);
        }

    }


}