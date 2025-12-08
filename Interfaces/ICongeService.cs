using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Systeme_RH.Models.Enums;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Interfaces
{
    public interface ICongeService
    {
        // --- Méthodes de lecture ---
        Task<CongeViewModel> GetByIdAsync(int id);
        // Pour qu'un employé voie SES congés
        Task<List<CongeViewModel>> GetByEmployeAsync(int employeId);
        
        // Pour les RH/Admin : vue globale avec filtres
        Task<CongeListViewModel> GetAllAsync(EtatDemandeConge? statut = null, TypeConge? type = null, int? employeId = null);
        
        // Pour le Dashboard RH : récupérer ce qui doit être traité
        Task<List<CongeViewModel>> GetDemandesEnAttenteAsync();

        // --- Méthodes d'action (Employé) ---
        Task<CongeViewModel> CreateDemandeAsync(CongeViewModel model, int employeId);
        Task<bool> CancelDemandeAsync(int congeId, int employeId);

        // --- Méthodes d'action (Validation RH) ---
        Task<bool> ValiderDemandeAsync(int congeId, int validateurId);
        Task<bool> RefuserDemandeAsync(int congeId, int validateurId, string motifRefus);

        // --- Méthodes Utilitaires & Métier ---
        // Statistiques pour le dashboard
        Task<Dictionary<string, int>> GetStatistiquesAsync();
        
        // Vérifie si l'employé a déjà posé un congé sur ces dates
        Task<bool> HasConflictAsync(int employeId, DateTime dateDebut, DateTime dateFin, int? excludeCongeId = null);
        
        // Calcule le nombre de jours ouvrés (hors weekends)
        int CalculerNombreJours(DateTime dateDebut, DateTime dateFin);

        Task<CongeRapportViewModel> GetRapportAsync();

    }
}
