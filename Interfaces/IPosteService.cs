using System.Collections.Generic;
using System.Threading.Tasks;
using Systeme_RH.Models.ViewModels;

namespace Systeme_RH.Interfaces
{
    public interface IPosteService
    {
        // Le paramètre optionnel permet de filtrer les postes d'un département spécifique
        Task<List<PosteViewModel>> GetAllAsync(int? departementId = null);
        
        Task<PosteViewModel> GetByIdAsync(int id);
        Task<PosteViewModel> CreateAsync(PosteViewModel model);
        Task<PosteViewModel> UpdateAsync(int id, PosteViewModel model);
        Task<bool> DeleteAsync(int id);
    }
}
