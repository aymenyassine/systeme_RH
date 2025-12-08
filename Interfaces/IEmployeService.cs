using System.Collections.Generic;
using System.Threading.Tasks;
using Systeme_RH.Models.ViewModels;
namespace Systeme_RH.Interfaces
{
    public interface IEmployeService
    {
        // Récupération liste filtrée (Recherche, Filtres)
        Task<List<EmployeViewModel>> GetAllAsync(string searchTerm, int? departementId, int? posteId, bool? estActif);

        // Récupération détail
        Task<EmployeViewModel> GetDetailsAsync(int id);
        Task<EmployeViewModel> GetByIdAsync(int id); // Souvent utilisé pour le Edit (peut être identique à GetDetails)

        // Actions CRUD
        Task<EmployeViewModel> CreateAsync(EmployeViewModel model);
        Task<EmployeViewModel> UpdateAsync(int id, EmployeViewModel model);
        Task DeleteAsync(int id);

        // Helpers pour les Dropdowns (Listes déroulantes)
        // Retourne un Dictionary ou une List<SelectListItem> simplifié (Key=Id, Value=Nom)
        Task<Dictionary<int, string>> GetDepartementsListAsync();
        Task<Dictionary<int, string>> GetPostesListAsync(int? departementId = null);
    }
}