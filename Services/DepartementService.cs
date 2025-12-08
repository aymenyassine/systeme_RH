using Microsoft.EntityFrameworkCore;
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Services
{
    public class DepartementService : IDepartementService
    {
        private readonly ApplicationDbContext _context;

        public DepartementService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. LECTURE ---

        public async Task<List<DepartementViewModel>> GetAllAsync()
        {
            return await _context.Departements
                .Include(d => d.Employes)
                .Include(d => d.Postes)
                .Select(d => new DepartementViewModel
                {
                    Id = d.Id,
                    Nom = d.Nom ?? "",
                    Description = d.Description,
                    // Calcul des stats en temps réel
                    NombreEmployes = d.Employes != null ? d.Employes.Count(e => e.ApplicationUser != null && e.ApplicationUser.EstActif) : 0,
                    NombrePostes = d.Postes != null ? d.Postes.Count : 0
                })
                .OrderBy(d => d.Nom)
                .ToListAsync();
        }

        public async Task<DepartementViewModel> GetByIdAsync(int id)
        {
            var d = await _context.Departements
                .Include(d => d.Employes)
                .Include(d => d.Postes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (d == null) return null!;

            return new DepartementViewModel
            {
                Id = d.Id,
                Nom = d.Nom ?? "",
                Description = d.Description,
                NombreEmployes = d.Employes?.Count(e => e.ApplicationUser != null && e.ApplicationUser.EstActif) ?? 0,
                NombrePostes = d.Postes?.Count ?? 0
            };
        }

        public async Task<List<EmployeViewModel>> GetEmployesByDepartementAsync(int departementId)
        {
            // Récupère la liste détaillée des employés d'un département spécifique
            return await _context.Employes
                .Include(e => e.Poste)
                .Include(e => e.ApplicationUser)
                .Where(e => e.DepartementId == departementId)
                .Select(e => new EmployeViewModel
                {
                    Id = e.Id,
                    Nom = e.Nom ?? string.Empty,
                    Prenom = e.Prenom ??  string.Empty,
                    Matricule = e.Matricule ??  string.Empty,
                    Email = e.EmailPersonnel ?? string.Empty,
                    PosteTitre = e.Poste!.Titre,
                    EstActif = e.ApplicationUser != null && e.ApplicationUser.EstActif
                })
                .OrderBy(e => e.Nom)
                .ToListAsync();
        }

        // --- 2. CRÉATION ---

        public async Task<DepartementViewModel> CreateAsync(DepartementViewModel model)
        {
            // Vérification unicité du nom
            if (await NomExistsAsync(model.Nom))
            {
                throw new InvalidOperationException($"Le département '{model.Nom}' existe déjà.");
            }

            var departement = new Departement
            {
                Nom = model.Nom,
                Description = model.Description
            };

            _context.Departements.Add(departement);
            await _context.SaveChangesAsync();

            model.Id = departement.Id;
            return model;
        }

        // --- 3. MODIFICATION ---

        public async Task<DepartementViewModel> UpdateAsync(int id, DepartementViewModel model)
        {
            var departement = await _context.Departements.FindAsync(id);
            if (departement == null) throw new KeyNotFoundException("Département introuvable.");

            // Vérification unicité du nom (en excluant l'ID actuel)
            if (await NomExistsAsync(model.Nom, id))
            {
                throw new InvalidOperationException($"Le département '{model.Nom}' existe déjà.");
            }

            departement.Nom = model.Nom;
            departement.Description = model.Description;

            await _context.SaveChangesAsync();
            return model;
        }

        // --- 4. SUPPRESSION ---

        public async Task<bool> DeleteAsync(int id)
        {
            var departement = await _context.Departements
                .Include(d => d.Employes) // Important pour la vérification
                .Include(d => d.Postes)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (departement == null) return false;

            // Règle métier : On ne supprime pas si des employés sont liés
            if (departement.Employes != null && departement.Employes.Any())
            {
                throw new InvalidOperationException("Impossible de supprimer ce département car il contient des employés.");
            }

            // Règle métier : On ne supprime pas si des postes sont liés
            if (departement.Postes != null && departement.Postes.Any())
            {
                throw new InvalidOperationException("Impossible de supprimer ce département car des postes y sont rattachés.");
            }

            _context.Departements.Remove(departement);
            await _context.SaveChangesAsync();
            return true;
        }

        // --- 5. VALIDATIONS ---

        public async Task<bool> NomExistsAsync(string nom, int? excludeId = null)
        {
            var query = _context.Departements.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return await query.AnyAsync(d => d.Nom == nom);
        }
    }
}