using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Controllers
{
    [Authorize(Roles = "RH, Admin")]
    public class EmployesController : Controller
    {
        private readonly IEmployeService _service;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public EmployesController(IEmployeService service, 
                                  IUserService userService, 
                                  ApplicationDbContext context)
        {
            _service = service;
            _userService = userService;
            _context = context;
        }

        // ==========================================
        // 1. LISTE (INDEX)
        // ==========================================
        public async Task<IActionResult> Index(string searchString)
        {
            var employes = await _service.GetAllAsync(searchString, null, null, null);
            return View(employes);
        }

        // ==========================================
        // 2. DÉTAILS
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employe = await _service.GetDetailsAsync(id.Value);

            if (employe == null) return NotFound();

            return View(employe);
        }

        // ==========================================
        // 3. CRÉATION (CREATE)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // CORRECTION : On charge TOUT en une seule fois
            await ChargerToutesLesListesPourCreation();
            
            return View(new EmployeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.CreateAsync(model);
                    
                    TempData["Success"] = "Employé créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // CORRECTION : Si échec, on recharge TOUTES les listes (y compris UnlinkedUsers)
            await ChargerToutesLesListesPourCreation(model.ApplicationUserId);

            return View(model);
        }

        // ==========================================
        // 4. MODIFICATION (EDIT)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employe = await _service.GetDetailsAsync(id.Value);
            if (employe == null) return NotFound();

            await ChargerListesDeroulantes(employe.DepartementId, employe.PosteId);
            return View(employe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    
                    await _service.UpdateAsync(id, model); 
                    TempData["Success"] = "Employé mis à jour avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (InvalidOperationException ex) 
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            await ChargerListesDeroulantes(model.DepartementId, model.PosteId);
            return View(model);
        }

        // ==========================================
        // 5. SUPPRESSION (DELETE)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employe = await _service.GetDetailsAsync(id.Value);
            if (employe == null) return NotFound();

            return View(employe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Employé supprimé (compte désactivé).";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. MÉTHODES PRIVÉES (Helpers)
        // ==========================================
        
        /// <summary>
        /// NOUVELLE MÉTHODE : Charge toutes les listes pour la création (avec UnlinkedUsers)
        /// </summary>
        private async Task ChargerToutesLesListesPourCreation(string? selectedUserId = null)
        {
            // 1. Listes Département et Poste
            var departements = await _context.Departements
                .OrderBy(d => d.Nom)
                .ToListAsync();
            
            var postes = await _context.Postes
                .OrderBy(p => p.Titre)
                .ToListAsync();

            ViewData["DepartementId"] = new SelectList(departements, "Id", "Nom");
            ViewData["PosteId"] = new SelectList(postes, "Id", "Titre");

            // 2. CORRECTION CRITIQUE : Liste des utilisateurs non liés
            var unlinkedUsers = await _userService.GetUnlinkedUsersAsync();
            
            // On crée une SelectList avec un format personnalisé pour afficher Email + Nom
            ViewBag.UnlinkedUsers = new SelectList(
                unlinkedUsers.Select(u => new 
                { 
                    Id = u.Id, 
                    Display = $"{u.Email} ({u.Prenom} {u.Nom})" 
                }),
                "Id", 
                "Display",
                selectedUserId
            );
        }

        /// <summary>
        /// Méthode pour Edit (pas besoin de UnlinkedUsers)
        /// </summary>
        private async Task ChargerListesDeroulantes(int? departementId = null, int? posteId = null)
        {
            var departements = await _context.Departements
                .OrderBy(d => d.Nom)
                .ToListAsync();
            
            var postes = await _context.Postes
                .OrderBy(p => p.Titre)
                .ToListAsync();

            ViewData["DepartementId"] = new SelectList(departements, "Id", "Nom", departementId);
            ViewData["PosteId"] = new SelectList(postes, "Id", "Titre", posteId);
        }
    }
}