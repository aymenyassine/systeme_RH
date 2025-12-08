using Microsoft.EntityFrameworkCore;
using Systeme_RH.Data;
using Systeme_RH.Interfaces;
using Systeme_RH.Models;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Services
{
    public class EmployeService : IEmployeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public EmployeService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // ==========================================
        // 1. LECTURE (GET)
        // ==========================================
        public async Task<List<EmployeViewModel>> GetAllAsync(string searchTerm, int? departementId, int? posteId, bool? estActif)
        {
            var query = _context.Employes
                .Include(e => e.Departement)
                .Include(e => e.Poste)
                .Include(e => e.ApplicationUser)
                .AsQueryable();

            // --- Filtres ---
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Nom!.Contains(searchTerm) || 
                                         e.Prenom!.Contains(searchTerm) || 
                                         e.Matricule!.Contains(searchTerm));
            }

            if (departementId.HasValue)
                query = query.Where(e => e.DepartementId == departementId.Value);

            if (posteId.HasValue)
                query = query.Where(e => e.PosteId == posteId.Value);

            if (estActif.HasValue)
            {
                if (estActif.Value)
                    query = query.Where(e => e.ApplicationUser != null && e.ApplicationUser.EstActif);
                else
                    query = query.Where(e => e.ApplicationUser == null || !e.ApplicationUser.EstActif);
            }

            // --- Mapping vers ViewModel ---
            return await query.Select(e => new EmployeViewModel
            {
                Id = e.Id,
                Matricule = e.Matricule!,
                Nom = e.Nom!,
                Prenom = e.Prenom!,
                Email = e.EmailPersonnel!,
                Telephone = e.Telephone,
                DateNaissance = e.DateNaissance,
                Adresse = e.Adresse,
                
                // CORRECTION : On mappe les champs financiers et RH
                SoldeConges = (decimal)e.SoldeConges, // Cast si nécessaire selon le type (double vs decimal)
                Salaire = e.Salaire,
                DateEmbauche = e.DateEmbauche,

                DepartementId = e.DepartementId,
                DepartementNom = e.Departement!.Nom,
                PosteId = e.PosteId,
                PosteTitre = e.Poste!.Titre,

                // CORRECTION : On mappe l'ID utilisateur pour voir le lien "OUI/NON"
                ApplicationUserId = e.ApplicationUserId,
                EstActif = e.ApplicationUser != null && e.ApplicationUser.EstActif
            }).ToListAsync();
        }

        public async Task<EmployeViewModel> GetByIdAsync(int id)
        {
            return await GetDetailsAsync(id);
        }

        public async Task<EmployeViewModel> GetDetailsAsync(int id)
        {
            var e = await _context.Employes
                .Include(x => x.Departement)
                .Include(x => x.Poste)
                .Include(x => x.ApplicationUser) // Important
                .FirstOrDefaultAsync(m => m.Id == id);

            if (e == null) return null!;

            return new EmployeViewModel
            {
                Id = e.Id,
                Matricule = e.Matricule!,
                Nom = e.Nom!,
                Prenom = e.Prenom!,
                Email = e.EmailPersonnel!,
                Telephone = e.Telephone,
                DateNaissance = e.DateNaissance,
                Adresse = e.Adresse,
                
                // Champs manquants ajoutés
                SoldeConges = (decimal)e.SoldeConges,
                Salaire = e.Salaire,
                DateEmbauche = e.DateEmbauche,

                DepartementId = e.DepartementId,
                DepartementNom = e.Departement!.Nom,
                PosteId = e.PosteId,
                PosteTitre = e.Poste!.Titre,

                // Lien User
                ApplicationUserId = e.ApplicationUserId,
                EstActif = e.ApplicationUser?.EstActif ?? false
            };
        }

        // ==========================================
        // 2. CRÉATION (CREATE)
        // ==========================================
        public async Task<EmployeViewModel> CreateAsync(EmployeViewModel model)
        {
            // Validations métier
            if (await _context.Employes.AnyAsync(e => e.EmailPersonnel == model.Email))
                throw new InvalidOperationException("Cet email est déjà utilisé.");
            
            if (await _context.Employes.AnyAsync(e => e.Matricule == model.Matricule))
                throw new InvalidOperationException("Ce matricule existe déjà.");

            // Création de l'entité
            var employe = new Employe
            {
                Matricule = model.Matricule,
                Nom = model.Nom,
                Prenom = model.Prenom,
                EmailPersonnel = model.Email,
                DateNaissance = model.DateNaissance,
                Telephone = model.Telephone,
                Adresse = model.Adresse,
                DepartementId = model.DepartementId,
                PosteId = model.PosteId,
                
                // Valeurs initiales
                DateEmbauche = model.DateEmbauche,
                Salaire = model.Salaire,
                SoldeConges = model.SoldeConges,

                // Si un user a été choisi dans la liste, on stocke l'ID tout de suite
                ApplicationUserId = model.ApplicationUserId 
            };

            _context.Employes.Add(employe);
            await _context.SaveChangesAsync(); // L'employé a maintenant un ID

            // --- LOGIQUE DE LIEN UTILISATEUR ---
            
            // CAS A : L'admin a choisi un utilisateur existant (Lien Manuel)
            if (!string.IsNullOrEmpty(model.ApplicationUserId))
            {
                // On met à jour la table AspNetUsers pour dire "Ce user appartient à cet employé"
                await _userService.LinkEmployeToUserAsync(model.ApplicationUserId, employe.Id);
            }
            // CAS B : Pas de choix -> Création Automatique
            else 
            {
                string passwordDefaut = $"Pass{model.Matricule}!";
                bool userCreated = await _userService.CreateUserForEmployeAsync(employe, passwordDefaut);
                
                if (!userCreated)
                {
                    // Optionnel : Logger l'erreur, mais on laisse l'employé créé
                }
            }

            model.Id = employe.Id;
            return model;
        }

        // ==========================================
        // 3. MODIFICATION (UPDATE)
        // ==========================================
        public async Task<EmployeViewModel> UpdateAsync(int id, EmployeViewModel model)
        {
            var employe = await _context.Employes.FindAsync(id);
            if (employe == null) throw new KeyNotFoundException("Employé introuvable.");

            // Vérification anti-doublon pour Matricule (si modifié)
            if (employe.Matricule != model.Matricule && 
                await _context.Employes.AnyAsync(e => e.Matricule == model.Matricule))
            {
                throw new InvalidOperationException("Ce matricule existe déjà.");
            }
            
            // Vérification anti-doublon pour Email (si modifié)
            if (employe.EmailPersonnel != model.Email &&
                await _context.Employes.AnyAsync(e => e.EmailPersonnel == model.Email))
            {
                throw new InvalidOperationException("Cet email est déjà utilisé.");
            }

            // --- Mise à jour des champs standards ---
            employe.Matricule = model.Matricule; // Correction: Ajouter le Matricule
            employe.Nom = model.Nom;
            employe.Prenom = model.Prenom;
            employe.EmailPersonnel = model.Email;
            employe.Telephone = model.Telephone;
            employe.Adresse = model.Adresse;
            employe.DateNaissance = model.DateNaissance; // Correction: Ajouter la date de naissance
            
            // Mise à jour des relations et données RH
            employe.DepartementId = model.DepartementId;
            employe.PosteId = model.PosteId;
            employe.Salaire = model.Salaire;
            employe.SoldeConges = model.SoldeConges;
            employe.DateEmbauche = model.DateEmbauche;
            
            // --------------------------------------------------------

            await _context.SaveChangesAsync();
            
            // --- LOGIQUE DE GESTION DU STATUT UTILISATEUR ---
            // Le statut "EstActif" du ViewModel correspond au statut du ApplicationUser lié.
            if (employe.ApplicationUserId != null)
            {
                // Si le statut dans le ViewModel est différent du statut actuel de l'utilisateur, on le change
                bool isUserActive = employe.ApplicationUser?.EstActif ?? false;
                if (model.EstActif != isUserActive)
                {
                    // La méthode de UserService va toggle (activer/désactiver) l'utilisateur
                    await _userService.ToggleUserStatusAsync(employe.ApplicationUserId);
                }
            }

            return model;
        }

        // ==========================================
        // 4. SUPPRESSION
        // ==========================================
        public async Task DeleteAsync(int id)
        {
            var employe = await _context.Employes.FindAsync(id);
            if (employe == null) return;

            // Désactiver le compte utilisateur lié
            if (employe.ApplicationUserId != null)
            {
                await _userService.ToggleUserStatusAsync(employe.ApplicationUserId);
            }
            
            // Optionnel : Supprimer physiquement l'employé ou utiliser un booléen EstSupprime
            _context.Employes.Remove(employe);
            await _context.SaveChangesAsync();
        }

        // ==========================================
        // 5. HELPERS
        // ==========================================
        public async Task<Dictionary<int, string>> GetDepartementsListAsync()
        {
            return await _context.Departements
                .OrderBy(d => d.Nom!)
                .ToDictionaryAsync(d => d.Id, d => d.Nom!);
        }

        public async Task<Dictionary<int, string>> GetPostesListAsync(int? departementId = null)
        {
            var query = _context.Postes.AsQueryable();

            if (departementId.HasValue)
                query = query.Where(p => p.DepartementId == departementId.Value);

            return await query
                .OrderBy(p => p.Titre!)
                .ToDictionaryAsync(p => p.Id, p => p.Titre!);
        }
    }
}