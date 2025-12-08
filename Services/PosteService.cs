using Microsoft.EntityFrameworkCore;
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Services
{
    public class PosteService : IPosteService
    {
        private readonly ApplicationDbContext _context;

        public PosteService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LECTURE ---

        public async Task<List<PosteViewModel>> GetAllAsync(int? departementId = null)
        {
            var query = _context.Postes
                .Include(p => p.Departement)
                .Include(p => p.Employes) // Pour compter les employés
                .AsQueryable();

            if (departementId.HasValue)
            {
                query = query.Where(p => p.DepartementId == departementId.Value);
            }

            return await query
                .Select(p => new PosteViewModel
                {
                    Id = p.Id,
                    Titre = p.Titre ?? string.Empty,
                    Description = p.Description,
                    Salaire = p.Salaire,
                    DepartementId = p.DepartementId,
                    DepartementNom = p.Departement!.Nom ?? "Non assigné",
                    // Compte le nombre d'employés actifs sur ce poste
                    NombreEmployes = p.Employes != null 
                        ? p.Employes.Count(e => e.ApplicationUser != null && e.ApplicationUser.EstActif) 
                        : 0
                })
                .OrderBy(p => p.Titre)
                .ToListAsync();
        }

        public async Task<PosteViewModel> GetByIdAsync(int id)
        {
            var p = await _context.Postes
                .Include(p => p.Departement)
                .Include(p => p.Employes)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null!;

            return new PosteViewModel
            {
                Id = p.Id,
                Titre = p.Titre!,
                Description = p.Description,
                Salaire = p.Salaire,
                DepartementId = p.DepartementId,
                DepartementNom = p.Departement?.Nom ?? "Non assigné",
                NombreEmployes = p.Employes?.Count(e => e.ApplicationUser != null && e.ApplicationUser.EstActif) ?? 0
            };
        }

        // --- CRÉATION ---

        public async Task<PosteViewModel> CreateAsync(PosteViewModel model)
        {
            var poste = new Poste
            {
                Titre = model.Titre,
                Description = model.Description,
                Salaire = model.Salaire,
                DepartementId = model.DepartementId
            };

            _context.Postes.Add(poste);
            await _context.SaveChangesAsync();

            model.Id = poste.Id;
            return model;
        }

        // --- MODIFICATION ---

        public async Task<PosteViewModel> UpdateAsync(int id, PosteViewModel model)
        {
            var poste = await _context.Postes.FindAsync(id);
            if (poste == null) throw new KeyNotFoundException("Poste introuvable.");

            poste.Titre = model.Titre;
            poste.Description = model.Description;
            poste.Salaire = model.Salaire;
            poste.DepartementId = model.DepartementId;

            await _context.SaveChangesAsync();
            return model;
        }

        // --- SUPPRESSION ---

        public async Task<bool> DeleteAsync(int id)
        {
            var poste = await _context.Postes
                .Include(p => p.Employes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poste == null) return false;

            // Règle métier : On ne supprime pas un poste s'il y a des employés dessus
            if (poste.Employes != null && poste.Employes.Any())
            {
                throw new InvalidOperationException("Impossible de supprimer ce poste car des employés l'occupent actuellement.");
            }

            _context.Postes.Remove(poste);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}