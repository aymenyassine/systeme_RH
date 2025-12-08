using System.ComponentModel.DataAnnotations;

namespace Systeme_RH.ViewModels
{
    // Modèle pour la LISTE (Index)
    public class UserListViewModel
    {
        public string? Id { get; set; }
        public string? NomComplet { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool EstActif { get; set; }
    }

    // Modèle pour la CRÉATION
    public class CreateUserViewModel
    {
        [Required]
        public string? Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string? Nom { get; set; }
        public string? Prenom { get; set; }

        [Display(Name = "Rôle")]
        public string? RoleSelectionne { get; set; }
    }

    // Modèle pour l'ÉDITION
    public class EditUserViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string? Email { get; set; }

        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public bool EstActif { get; set; }

        [Display(Name = "Rôle")]
        public string? RoleSelectionne { get; set; }
    }
    public class AdminResetPasswordViewModel
    {
        public string? Id { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string? NewPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string? ConfirmPassword { get; set; }
    }
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }
    }
}
