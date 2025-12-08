using System.Threading.Tasks;
using Systeme_RH.Models; // Pour accéder à la classe Employe et ApplicationUser

namespace Systeme_RH.Interfaces
{
    public interface IUserService
    {
        // Crée le compte Identity pour un nouvel employé et simule l'envoi d'email
        // Retourne true si succès, false sinon
        Task<bool> CreateUserForEmployeAsync(Employe employe, string motDePasseProvisoire);

        // Active ou Désactive un compte ApplicationUser (Blocage)
        Task<bool> ToggleUserStatusAsync(string userId);

        // Récupère l'ApplicationUser lié à un ID d'employé
        Task<ApplicationUser?> GetByEmployeIdAsync(int employeId);

        // Réinitialisation du mot de passe (Admin)
        Task<bool> ResetPasswordAsync(string userId, string nouveauMotDePasse);

        // Ajoutez cette méthode
        Task<List<ApplicationUser>> GetUnlinkedUsersAsync();
        Task<bool> LinkEmployeToUserAsync(string userId, int employeId);
    }
}