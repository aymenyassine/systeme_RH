using System.Collections.Generic;
using System.Threading.Tasks;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Interfaces
{
    public interface IDepartementService
    {
        Task<List<DepartementViewModel>> GetAllAsync();
        Task<DepartementViewModel> GetByIdAsync(int id);
        
        Task<DepartementViewModel> CreateAsync(DepartementViewModel model);
        Task<DepartementViewModel> UpdateAsync(int id, DepartementViewModel model);
        Task<bool> DeleteAsync(int id);
        
        // Validations métier
        Task<bool> NomExistsAsync(string nom, int? excludeId = null);
        
        // Récupération des employés liés (pour l'affichage détail)
        Task<List<EmployeViewModel>> GetEmployesByDepartementAsync(int departementId);
    }
}