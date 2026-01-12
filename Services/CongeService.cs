using Microsoft.EntityFrameworkCore;
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models;
using Systeme_RH.Models.Enums;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Services
{
    public class CongeService : ICongeService
    {
        private readonly ApplicationDbContext _context;

        public CongeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LECTURE ---

        public async Task<CongeViewModel> GetByIdAsync(int id)
        {
            var conge = await _context.DemandesConges // Correction du nom de la table
                .Include(c => c.Employe)
                .ThenInclude(e => e!.Departement)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conge == null)
                throw new InvalidOperationException($"Demande de congé avec l'ID {id} introuvable.");

            return MapToViewModel(conge);
        }

        public async Task<List<CongeViewModel>> GetByEmployeAsync(int employeId)
        {
            var conges = await _context.DemandesConges
                .Where(c => c.EmployeId == employeId)
                .OrderByDescending(c => c.DateDemande)
                .ToListAsync();

            return conges.Select(MapToViewModel).ToList();
        }

        // CORRECTION ICI : Utilisation de EtatDemandeConge pour coller à l'interface
        public async Task<CongeListViewModel> GetAllAsync(EtatDemandeConge? statut = null, TypeConge? type = null, int? employeId = null)
        {
            var query = _context.DemandesConges
                .Include(c => c.Employe)
                .AsQueryable();

            if (statut.HasValue)
                query = query.Where(c => c.Etat == statut.Value);

            if (type.HasValue)
                query = query.Where(c => c.TypeConge == type.Value);

            if (employeId.HasValue)
                query = query.Where(c => c.EmployeId == employeId.Value);

            var list = await query.OrderByDescending(c => c.DateDemande).ToListAsync();

            // Calcul des stats en temps réel
            return new CongeListViewModel
            {
                Conges = list.Select(MapToViewModel).ToList(),
                StatutFilter = statut,
                TypeFilter = type,
                EmployeIdFilter = employeId,
                TotalDemandes = await _context.DemandesConges.CountAsync(),
                // Utilisation de EtatDemandeConge
                DemandesEnAttente = await _context.DemandesConges.CountAsync(c => c.Etat == EtatDemandeConge.EnAttente),
                DemandesValidees = await _context.DemandesConges.CountAsync(c => c.Etat == EtatDemandeConge.Approuve),
                DemandesRefusees = await _context.DemandesConges.CountAsync(c => c.Etat == EtatDemandeConge.Refuse)
            };
        }

        public async Task<List<CongeViewModel>> GetDemandesEnAttenteAsync()
        {
            var conges = await _context.DemandesConges
                .Include(c => c.Employe)
                .ThenInclude(e => e!.Departement)
                .Where(c => c.Etat == EtatDemandeConge.EnAttente) // Correction Enum
                .OrderBy(c => c.DateDemande)
                .ToListAsync();

            return conges.Select(MapToViewModel).ToList();
        }

        // --- ACTIONS EMPLOYÉ ---

        public async Task<CongeViewModel> CreateDemandeAsync(CongeViewModel model, int employeId)
        {
            if (model.DateFin < model.DateDebut)
                throw new InvalidOperationException("La date de fin doit être postérieure à la date de début.");

            int jours = CalculerNombreJours(model.DateDebut, model.DateFin);
            if (jours <= 0)
                throw new InvalidOperationException("La durée du congé doit être d'au moins 1 jour ouvré.");

            var employe = await _context.Employes.FindAsync(employeId);
            if (employe == null) throw new Exception("Employé introuvable.");

            if (model.Type == TypeConge.Paye)
            {
                if (employe.SoldeConges < jours)
                {
                    throw new InvalidOperationException($"Solde insuffisant. Solde actuel : {employe.SoldeConges}, demandé : {jours}.");
                }
            }

            if (await HasConflictAsync(employeId, model.DateDebut, model.DateFin))
                throw new InvalidOperationException("Une demande existe déjà pour cette période.");

            var demande = new DemandeConge
            {
                EmployeId = employeId,
                TypeConge = model.Type,
                DateDebut = model.DateDebut,
                DateFin = model.DateFin,
                NombreJours = jours,
                Motif = model.Motif,
                Etat = EtatDemandeConge.EnAttente, // Correction Enum
                DateDemande = DateTime.Now
            };

            _context.DemandesConges.Add(demande);
            await _context.SaveChangesAsync();

            return MapToViewModel(demande);
        }

        public async Task<bool> CancelDemandeAsync(int congeId, int employeId)
        {
            var demande = await _context.DemandesConges.FindAsync(congeId);

            if (demande == null || demande.EmployeId != employeId) return false;

            if (demande.Etat != EtatDemandeConge.EnAttente) // Correction Enum
                throw new InvalidOperationException("Impossible d'annuler une demande déjà traitée.");

            demande.Etat = EtatDemandeConge.Annule; // Correction Enum
            await _context.SaveChangesAsync();
            return true;
        }

        // --- ACTIONS RH ---

        public async Task<bool> ValiderDemandeAsync(int congeId, int validateurId)
        {
            var demande = await _context.DemandesConges
                                .Include(d => d.Employe)
                                .FirstOrDefaultAsync(d => d.Id == congeId);

            if (demande == null) return false;

            if (demande.Etat != EtatDemandeConge.EnAttente)
                throw new InvalidOperationException("Cette demande a déjà été traitée.");

            if (demande.TypeConge == TypeConge.Paye)
            {
                if (demande.Employe == null)
                    throw new InvalidOperationException("Employé introuvable pour cette demande.");

                if (demande.Employe.SoldeConges < demande.NombreJours)
                    throw new InvalidOperationException("Solde insuffisant.");

                demande.Employe.SoldeConges -= demande.NombreJours;
            }

            demande.Etat = EtatDemandeConge.Approuve; // Correction Enum
            demande.DateTraitement = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RefuserDemandeAsync(int congeId, int validateurId, string motifRefus)
        {
            var demande = await _context.DemandesConges.FindAsync(congeId);
            if (demande == null) return false;

            if (demande.Etat != EtatDemandeConge.EnAttente)
                throw new InvalidOperationException("Cette demande a déjà été traitée.");

            demande.Etat = EtatDemandeConge.Refuse; // Correction Enum
            demande.DateTraitement = DateTime.Now;
            demande.CommentaireRH = motifRefus;

            await _context.SaveChangesAsync();
            return true;
        }

        // --- UTILITAIRES ---

        public async Task<Dictionary<string, int>> GetStatistiquesAsync()
        {
            return new Dictionary<string, int>
            {
                { "EnAttente", await _context.DemandesConges.CountAsync(c => c.Etat == EtatDemandeConge.EnAttente) },
                { "Approuve", await _context.DemandesConges.CountAsync(c => c.Etat == EtatDemandeConge.Approuve) },
                { "Refuse", await _context.DemandesConges.CountAsync(c => c.Etat == EtatDemandeConge.Refuse) }
            };
        }

        public async Task<bool> HasConflictAsync(int employeId, DateTime dateDebut, DateTime dateFin, int? excludeCongeId = null)
        {
            var query = _context.DemandesConges
                .Where(c => c.EmployeId == employeId &&
                            c.Etat != EtatDemandeConge.Annule &&
                            c.Etat != EtatDemandeConge.Refuse &&
                            c.DateDebut <= dateFin &&
                            c.DateFin >= dateDebut);

            if (excludeCongeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCongeId.Value);
            }

            return await query.AnyAsync();
        }

        public int CalculerNombreJours(DateTime dateDebut, DateTime dateFin)
        {
            int jours = 0;
            for (var date = dateDebut; date <= dateFin; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    jours++;
                }
            }
            return jours;
        }

        private CongeViewModel MapToViewModel(DemandeConge entity)
        {
            return new CongeViewModel
            {
                Id = entity.Id,
                Type = entity.TypeConge,
                DateDebut = entity.DateDebut,
                DateFin = entity.DateFin,
                NombreJours = entity.NombreJours,
                Motif = entity.Motif ?? string.Empty,
                Statut = entity.Etat, // Cela doit bien matcher avec EtatDemandeConge
                DateDemande = entity.DateDemande,
                DateValidation = entity.DateTraitement,
                MotifRefus = entity.CommentaireRH,
                EmployeId = entity.EmployeId,
                EmployeNom = entity.Employe != null ? $"{entity.Employe.Prenom} {entity.Employe.Nom}" : "",
                EmployeMatricule = entity.Employe?.Matricule ?? "",
                EmployeDepartement = entity.Employe?.Departement?.Nom ?? "",
                EmployeSoldeConges = entity.Employe != null ? entity.Employe.SoldeConges : 0
            };
        }

        public async Task<CongeRapportViewModel> GetRapportAsync()
        {
            var baseQuery = _context.DemandesConges
                .Include(c => c.Employe)
                .ThenInclude(e => e!.Departement);

            return new CongeRapportViewModel
            {
                TotalDemandes = await baseQuery.CountAsync(),
                EnAttente = await baseQuery.CountAsync(c => c.Etat == EtatDemandeConge.EnAttente),
                Approuvees = await baseQuery.CountAsync(c => c.Etat == EtatDemandeConge.Approuve),
                Refusees = await baseQuery.CountAsync(c => c.Etat == EtatDemandeConge.Refuse),
                Annulees = await baseQuery.CountAsync(c => c.Etat == EtatDemandeConge.Annule),

                ParType = await baseQuery
                    .GroupBy(c => c.TypeConge)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.Count),

                ParDepartement = await baseQuery
                    .Where(c => c.Employe!.Departement != null)
                    .GroupBy(c => c.Employe!.Departement!.Nom)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key ?? "", x => x.Count)
            };
        }

    }
}
