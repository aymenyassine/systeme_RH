using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Systeme_RH.Interfaces;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Controllers
{
    [Authorize(Roles = "RH, Admin")] // Seuls les RH et Admin gèrent les contrats
    public class ContratsController : Controller
    {
        private readonly IContratService _contratService;
        private readonly IWebHostEnvironment _webHostEnvironment; // Pour gérer l'upload de fichiers

        public ContratsController(IContratService contratService, IWebHostEnvironment webHostEnvironment)
        {
            _contratService = contratService;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 1. INDEX (Liste des contrats d'un employé)
        // ==========================================
        // 1. Changez int en int? (nullable) pour accepter l'absence d'ID
        [HttpGet]
        public async Task<IActionResult> Index(int? employeId)
        {
            IEnumerable<ContratViewModel> contrats;

            if (employeId.HasValue && employeId.Value > 0)
            {
                // Cas 1 : On veut les contrats d'un employé spécifique
                contrats = await _contratService.GetByEmployeIdAsync(employeId.Value);
                ViewBag.EmployeId = employeId.Value;
                ViewBag.EmployeNom = contrats.FirstOrDefault()?.EmployeNom ?? "l'employé";
                ViewBag.IsGlobalList = false; // Pour gérer l'affichage du bouton "Créer"
            }
            else
            {
                // Cas 2 : On vient du Dashboard, on veut TOUS les contrats
                // Assurez-vous d'avoir une méthode GetAllAsync dans votre service
                contrats = await _contratService.GetAllAsync(); 
                ViewBag.EmployeId = null;
                ViewBag.EmployeNom = "Tous les employés";
                ViewBag.IsGlobalList = true;
            }

            return View(contrats);
        }
        // ==========================================
        // 2. DETAILS
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var contrat = await _contratService.GetByIdAsync(id);
            if (contrat == null) return NotFound();

            return View(contrat);
        }

        // ==========================================
        // 3. CREATE
        // ==========================================
        [HttpGet]
        public IActionResult Create(int employeId)
        {
            // On pré-remplit l'ID de l'employé et la date du jour
            return View(new ContratViewModel 
            { 
                EmployeId = employeId, 
                DateDebut = DateTime.Today,
                EstActif = true 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContratViewModel model, IFormFile? fichierJoint)
        {
            if (ModelState.IsValid)
            {
                // Gestion de l'Upload du fichier (PDF/Image)
                if (fichierJoint != null && fichierJoint.Length > 0)
                {
                    model.PieceJointeUrl = await UploadFile(fichierJoint);
                }

                await _contratService.CreateAsync(model);
                
                TempData["Success"] = "Nouveau contrat créé avec succès.";
                return RedirectToAction(nameof(Index), new { employeId = model.EmployeId });
            }

            return View(model);
        }

        // ==========================================
        // 4. EDIT (Modification)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var contrat = await _contratService.GetByIdAsync(id);
            if (contrat == null) return NotFound();

            return View(contrat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContratViewModel model, IFormFile? fichierJoint)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Si un nouveau fichier est uploadé, on remplace l'ancien
                if (fichierJoint != null && fichierJoint.Length > 0)
                {
                    // Optionnel : Supprimer l'ancien fichier ici si nécessaire
                    model.PieceJointeUrl = await UploadFile(fichierJoint);
                }
                else 
                {
                    // Important : Si pas de nouveau fichier, on doit garder l'URL de l'ancien
                    // (Cela nécessite de récupérer l'ancien modèle ou de le stocker dans un champ hidden dans la vue)
                    var ancienContrat = await _contratService.GetByIdAsync(id);
                    model.PieceJointeUrl = ancienContrat?.PieceJointeUrl;
                }

                try
                {
                    await _contratService.UpdateAsync(id, model);
                    TempData["Success"] = "Contrat mis à jour.";
                    return RedirectToAction(nameof(Index), new { employeId = model.EmployeId });
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
            }
            return View(model);
        }

        // ==========================================
        // 5. TERMINER CONTRAT (Clôture)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Terminer(int id)
        {
            var contrat = await _contratService.GetByIdAsync(id);
            if (contrat == null) return NotFound();

            // On affiche une vue spécifique pour demander la date de fin
            return View(contrat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmerTerminer(int id, DateTime dateFin)
        {
            try
            {
                bool result = await _contratService.TerminerContratAsync(id, dateFin);
                if (result)
                {
                    TempData["Success"] = "Le contrat a été clôturé.";
                    
                    // On doit récupérer l'ID employe pour la redirection, 
                    // on peut le faire via le service avant la clôture ou le passer en paramètre
                    var contrat = await _contratService.GetByIdAsync(id); 
                    return RedirectToAction(nameof(Index), new { employeId = contrat?.EmployeId });
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                // Retourner à la vue avec l'erreur
                var contrat = await _contratService.GetByIdAsync(id);
                return View("Terminer", contrat);
            }

            return RedirectToAction(nameof(Index)); // Fallback
        }

        // ==========================================
        // 6. DELETE (Suppression physique - Admin seulement ?)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Extra sécurité
        public async Task<IActionResult> Delete(int id)
        {
            var contrat = await _contratService.GetByIdAsync(id);
            if (contrat == null) return NotFound();

            int employeId = contrat.EmployeId; // Pour la redirection

            // Suppression du fichier physique si nécessaire
            if (!string.IsNullOrEmpty(contrat.PieceJointeUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, contrat.PieceJointeUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _contratService.DeleteAsync(id);
            
            TempData["Success"] = "Contrat supprimé définitivement.";
            return RedirectToAction(nameof(Index), new { employeId = employeId });
        }


        // ==========================================
        // FONCTION PRIVÉE : UPLOAD
        // ==========================================
        private async Task<string> UploadFile(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "contrats");
            
            // Créer le dossier s'il n'existe pas
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Nom unique pour éviter les doublons
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Retourne le chemin relatif pour la BDD (ex: /uploads/contrats/monfichier.pdf)
            return "/uploads/contrats/" + uniqueFileName;
        }
    }
}