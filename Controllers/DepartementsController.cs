using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Systeme_RH.Interfaces;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Controllers
{
    [Authorize(Roles = "RH, Admin")] // Sécurisé pour RH et Admin
    public class DepartementsController : Controller
    {
        private readonly IDepartementService _service;

        public DepartementsController(IDepartementService service)
        {
            _service = service;
        }

        // ==========================================
        // 1. INDEX
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var departements = await _service.GetAllAsync();
            return View(departements);
        }

        // ==========================================
        // 2. DETAILS
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var departement = await _service.GetByIdAsync(id);
            if (departement == null) return NotFound();

            // Optionnel : On peut aussi charger la liste des employés pour l'afficher en bas de page
            ViewBag.Employes = await _service.GetEmployesByDepartementAsync(id);

            return View(departement);
        }

        // ==========================================
        // 3. CREATE
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartementViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.CreateAsync(model);
                    TempData["Success"] = "Département créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    // On capture l'erreur "Le département existe déjà" venant du Service
                    ModelState.AddModelError("Nom", ex.Message);
                }
            }
            return View(model);
        }

        // ==========================================
        // 4. EDIT
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var departement = await _service.GetByIdAsync(id);
            if (departement == null) return NotFound();
            return View(departement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartementViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // CORRECTION ICI : On passe bien les 2 paramètres (id, model)
                    await _service.UpdateAsync(id, model);
                    TempData["Success"] = "Département mis à jour.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (InvalidOperationException ex)
                {
                    // Erreur de doublon de nom
                    ModelState.AddModelError("Nom", ex.Message);
                }
            }
            return View(model);
        }

        // ==========================================
        // 5. DELETE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var departement = await _service.GetByIdAsync(id);
            if (departement == null) return NotFound();
            return View(departement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                bool result = await _service.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Département supprimé.";
                }
                else
                {
                    TempData["Error"] = "Département introuvable.";
                }
            }
            catch (InvalidOperationException ex)
            {
                // Capture l'erreur "Impossible de supprimer car contient des employés"
                TempData["Error"] = ex.Message;
                
                // On redirige vers la vue Delete pour afficher l'erreur
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}