using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Systeme_RH.Models.ViewModels; // Assure-toi d'avoir tes ViewModels ici

namespace Systeme_RH.Interfaces
{
    public interface IContratService
    {
        // Récupère l'historique des contrats d'un employé spécifique
        Task<List<ContratViewModel>> GetByEmployeIdAsync(int employeId);

        // Récupère un contrat précis pour modification (Edit)
        Task<ContratViewModel?> GetByIdAsync(int id);

        // Récupère le contrat ACTIF (en cours) d'un employé (utile pour vérifier s'il est sous contrat)
        Task<ContratViewModel?> GetContratActifAsync(int employeId);

        // Crée un nouveau contrat
        Task CreateAsync(ContratViewModel model);

        // Modifie un contrat existant (ex: correction salaire, changement type)
        Task UpdateAsync(int id, ContratViewModel model);

        // Clôture un contrat (Mettre fin au contrat avec une date de sortie)
        Task<bool> TerminerContratAsync(int id, DateTime dateFin);
        
        // Supprime un contrat (cas d'erreur de saisie, Admin seulement)
        Task DeleteAsync(int id);
        Task<IEnumerable<ContratViewModel>> GetAllAsync();
    }
}