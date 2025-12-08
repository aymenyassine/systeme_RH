using System.ComponentModel.DataAnnotations;

namespace Systeme_RH.Models.ViewModels
{
    public class EmployeViewModel
    {
        public int Id { get; set; }

        // --- INFORMATIONS PERSONNELLES ---

        [Required(ErrorMessage = "Le matricule est obligatoire.")]
        [Display(Name = "Matricule")]
        public string Matricule { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        // Propriété calculée pour l'affichage facile (lecture seule)
        [Display(Name = "Nom Complet")]
        public string NomComplet => $"{Prenom} {Nom}";

        [Required(ErrorMessage = "La date de naissance est requise.")]
        [DataType(DataType.Date)] // Affiche un calendrier HTML5
        [Display(Name = "Date de naissance")]
        public DateTime DateNaissance { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        [Display(Name = "Email Professionnel")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Téléphone")]
        [Phone]
        public string? Telephone { get; set; }

        [Display(Name = "Adresse")]
        public string? Adresse { get; set; }

        // --- INFORMATIONS RH & CONTRAT ---

        [Display(Name = "Solde de congés")]
        public decimal SoldeConges { get; set; }

        [Display(Name = "Statut du compte")]
        public bool EstActif { get; set; } = true;

        // --- RELATIONS (Pour les listes déroulantes) ---

        [Required(ErrorMessage = "Veuillez sélectionner un département.")]
        [Display(Name = "Département")]
        public int DepartementId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date d'embauche")]
        public DateTime DateEmbauche { get; set; } = DateTime.Today; // Valeur par défaut

        [Range(0, 999999, ErrorMessage = "Le salaire doit être positif")]
        [Display(Name = "Salaire Mensuel")]
        public decimal Salaire { get; set; }

        // Juste pour l'affichage dans la liste (pas besoin de le saisir)
        [Display(Name = "Département")]
        public string? DepartementNom { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un poste.")]
        [Display(Name = "Poste")]
        public int PosteId { get; set; }

        // Juste pour l'affichage dans la liste
        [Display(Name = "Poste")]
        public string? PosteTitre { get; set; }

        public string? ApplicationUserId { get; set; }
    }
}