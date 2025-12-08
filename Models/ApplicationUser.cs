using Microsoft.AspNetCore.Identity;
using Systeme_RH.Models;

namespace Systeme_RH.Models
{
    // Cette classe étend l'ApplicationUser par défaut de .NET Identity
    public class ApplicationUser : IdentityUser
    {
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        
        // Date de création du compte (utile pour l'admin)
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        
        // Est-ce que le compte est actif ?
        public bool EstActif { get; set; } = true;

        // Lien Optionnel : Un User peut être lié à une fiche Employé (L'admin n'en a pas forcément)
        public int? EmployeId { get; set; }
        public virtual Employe? Employe { get; set; }
    }
}