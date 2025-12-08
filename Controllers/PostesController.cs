using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Nécessaire pour charger la liste des départements
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Controllers
{
    [Authorize(Roles = "RH, Admin")] // Sécurisation : Seuls RH et Admin gèrent les postes
    public class PostesController : Controller
    {
        private readonly IPosteService _posteService;
        private readonly ApplicationDbContext _context; // Utilisé uniquement pour les listes déroulantes

        public PostesController(IPosteService posteService, ApplicationDbContext context)
        {
            _posteService = posteService;
            _context = context;
        }

        // ==========================================
        // 1. INDEX (Avec filtre par département)
        // ==========================================
        public async Task<IActionResult> Index(int? departementId)
        {
            var postes = await _posteService.GetAllAsync(departementId);

            // Pour le filtre dans la Vue :
            ViewData["DepartementId"] = new SelectList(_context.Departements, "Id", "Nom", departementId);
            
            return View(postes);
        }

        // ==========================================
        // 2. DETAILS
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var poste = await _posteService.GetByIdAsync(id);
            if (poste == null) return NotFound();

            return View(poste);
        }

        // ==========================================
        // 3. CREATE
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["DepartementId"] = new SelectList(_context.Departements, "Id", "Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PosteViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _posteService.CreateAsync(model);
                TempData["Success"] = "Le poste a été créé avec succès.";
                return RedirectToAction(nameof(Index));
            }

            // En cas d'erreur, on recharge la liste
            ViewData["DepartementId"] = new SelectList(_context.Departements, "Id", "Nom", model.DepartementId);
            return View(model);
        }

        // ==========================================
        // 4. EDIT
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var poste = await _posteService.GetByIdAsync(id);
            if (poste == null) return NotFound();

            ViewData["DepartementId"] = new SelectList(_context.Departements, "Id", "Nom", poste.DepartementId);
            return View(poste);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PosteViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _posteService.UpdateAsync(id, model);
                    TempData["Success"] = "Le poste a été mis à jour.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
            }

            ViewData["DepartementId"] = new SelectList(_context.Departements, "Id", "Nom", model.DepartementId);
            return View(model);
        }

        // ==========================================
        // 5. DELETE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var poste = await _posteService.GetByIdAsync(id);
            if (poste == null) return NotFound();

            return View(poste);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                bool result = await _posteService.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Le poste a été supprimé.";
                }
                else
                {
                    TempData["Error"] = "Le poste n'a pas été trouvé.";
                }
            }
            catch (InvalidOperationException ex)
            {
                // C'est ici qu'on attrape l'erreur "Impossible de supprimer ce poste car des employés l'occupent"
                // On redirige vers la page Delete (ou Index) avec le message d'erreur
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}