using Microsoft.EntityFrameworkCore;
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Services
{
    public class ContratService : IContratService
    {
        private readonly ApplicationDbContext _context;

        public ContratService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LECTURE ---

        public async Task<List<ContratViewModel>> GetByEmployeIdAsync(int employeId)
        {
            // On récupère l'historique complet, du plus récent au plus ancien
            return await _context.Contrats
                .Include(c => c.Employe)
                .Where(c => c.EmployeId == employeId)
                .OrderByDescending(c => c.DateDebut)
                .Select(c => new ContratViewModel
                {
                    Id = c.Id,
                    DateDebut = c.DateDebut,
                    DateFin = c.DateFin,
                    SalaireMensuel = c.SalaireMensuel,
                    TypeContrat = c.TypeContrat,
                    PieceJointeUrl = c.PieceJointeUrl,
                    EstActif = c.EstActif,
                    EmployeId = c.EmployeId,
                    EmployeNom = c.Employe!.Nom + " " + c.Employe.Prenom
                })
                .ToListAsync();
        }

        public async Task<ContratViewModel?> GetByIdAsync(int id)
        {
            var c = await _context.Contrats
                .Include(c => c.Employe)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return null;

            return new ContratViewModel
            {
                Id = c.Id,
                DateDebut = c.DateDebut,
                DateFin = c.DateFin,
                SalaireMensuel = c.SalaireMensuel,
                TypeContrat = c.TypeContrat,
                PieceJointeUrl = c.PieceJointeUrl,
                EstActif = c.EstActif,
                EmployeId = c.EmployeId,
                EmployeNom = c.Employe!.Nom + " " + c.Employe.Prenom
            };
        }

        public async Task<ContratViewModel?> GetContratActifAsync(int employeId)
        {
            var c = await _context.Contrats
                .Where(x => x.EmployeId == employeId && x.EstActif)
                .OrderByDescending(x => x.DateDebut)
                .FirstOrDefaultAsync();

            if (c == null) return null;

            // Mapping rapide
            return new ContratViewModel
            {
                Id = c.Id,
                TypeContrat = c.TypeContrat,
                SalaireMensuel = c.SalaireMensuel,
                DateDebut = c.DateDebut
            };
        }

        // --- CRÉATION ---

        public async Task CreateAsync(ContratViewModel model)
        {
            // Règle métier : Si on crée un nouveau contrat actif, on devrait peut-être
            // clôturer automatiquement l'ancien ? Pour l'instant on laisse le choix au RH.

            var contrat = new Contrat
            {
                EmployeId = model.EmployeId,
                TypeContrat = model.TypeContrat,
                DateDebut = model.DateDebut,
                DateFin = model.DateFin,
                SalaireMensuel = model.SalaireMensuel,
                PieceJointeUrl = model.PieceJointeUrl,
                EstActif = true // Actif par défaut à la création
            };

            _context.Contrats.Add(contrat);
            await _context.SaveChangesAsync();
        }

        // --- MODIFICATION ---

        public async Task UpdateAsync(int id, ContratViewModel model)
        {
            var contrat = await _context.Contrats.FindAsync(id);
            if (contrat == null) throw new KeyNotFoundException("Contrat introuvable.");

            contrat.DateDebut = model.DateDebut;
            contrat.DateFin = model.DateFin;
            contrat.SalaireMensuel = model.SalaireMensuel;
            contrat.TypeContrat = model.TypeContrat;
            
            if (model.PieceJointeUrl != null)
            {
                contrat.PieceJointeUrl = model.PieceJointeUrl;
            }

            await _context.SaveChangesAsync();
        }

        // --- CLÔTURE (FIN DE CONTRAT) ---

        public async Task<bool> TerminerContratAsync(int id, DateTime dateFin)
        {
            var contrat = await _context.Contrats.FindAsync(id);
            if (contrat == null) return false;

            if (dateFin < contrat.DateDebut)
                throw new InvalidOperationException("La date de fin ne peut pas être avant la date de début.");

            contrat.DateFin = dateFin;
            contrat.EstActif = false; // On désactive le contrat

            await _context.SaveChangesAsync();
            return true;
        }

        // --- SUPPRESSION (Cas d'erreur seulement) ---

        public async Task DeleteAsync(int id)
        {
            var contrat = await _context.Contrats.FindAsync(id);
            if (contrat != null)
            {
                _context.Contrats.Remove(contrat);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ContratViewModel>> GetAllAsync()
        {
            return await _context.Contrats
                .Include(c => c.Employe) // Important pour afficher le nom de l'employé
                .Select(c => new ContratViewModel
                {
                    Id = c.Id,
                    EmployeId = c.EmployeId,
                    EmployeNom = c.Employe!.Nom + " " + c.Employe.Prenom, // On récupère le nom complet
                    TypeContrat = c.TypeContrat,
                    DateDebut = c.DateDebut,
                    DateFin = c.DateFin,
                    SalaireMensuel = c.SalaireMensuel,
                    EstActif = c.EstActif,
                    PieceJointeUrl = c.PieceJointeUrl
                })
                .ToListAsync();
        }
        
    }
}