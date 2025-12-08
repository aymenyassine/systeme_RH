using System.ComponentModel.DataAnnotations;

namespace Systeme_RH.Models.ViewModels
{
    public class DepartementViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du département est requis.")]
        [StringLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères.")]
        [Display(Name = "Nom du département")]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La description ne peut dépasser 500 caractères.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // --- GESTION DU RESPONSABLE (Optionnel) ---

        [Display(Name = "Responsable du département")]
        public int? ResponsableId { get; set; }

        // Pour l'affichage seulement (ex: "Ahmed Benali")
        [Display(Name = "Responsable")]
        public string? ResponsableNom { get; set; }

        // --- STATISTIQUES (Lecture seule pour l'Index) ---

        [Display(Name = "Effectif")]
        public int NombreEmployes { get; set; }

        [Display(Name = "Postes ouverts")]
        public int NombrePostes { get; set; }
    }
}