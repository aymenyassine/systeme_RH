using System.ComponentModel.DataAnnotations;

namespace Systeme_RH.Models.ViewModels
{
    public class PosteViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le titre du poste est requis.")]
        [StringLength(100, ErrorMessage = "Le titre ne peut dépasser 100 caractères.")]
        [Display(Name = "Titre du poste")]
        public string Titre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Le salaire doit être positif.")]
        [Display(Name = "Salaire")]
        public decimal Salaire { get; set; }

        // --- RELATION DÉPARTEMENT ---

        [Required(ErrorMessage = "Veuillez sélectionner un département.")]
        [Display(Name = "Département")]
        public int DepartementId { get; set; }

        // Pour l'affichage dans la liste (ex: "IT")
        [Display(Name = "Département")]
        public string? DepartementNom { get; set; }

        // --- STATISTIQUES ---
        
        [Display(Name = "Employés actuels")]
        public int NombreEmployes { get; set; }
    }
}