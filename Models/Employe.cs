using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Systeme_RH.Models
{
    public class Employe
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Matricule")]
        public string? Matricule { get; set; } 

        [Required(ErrorMessage = "Le nom est requis.")]
        public string? Nom { get; set; }
        
        [Required(ErrorMessage = "Le prénom est requis.")]
        public string? Prenom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de Naissance")]
        public DateTime DateNaissance { get; set; }
        
        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress]
        [Display(Name = "Email Personnel")]
        public string? EmailPersonnel { get; set; } 

        public string? Telephone { get; set; }
        public string? Adresse { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date d'embauche")]
        public DateTime DateEmbauche { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salaire")]
        public decimal Salaire { get; set; }

        // --- GESTION CONGÉS ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoldeConges { get; set; } = 0; 

        // --- RELATIONS ---
        [Display(Name = "Poste")]
        public int PosteId { get; set; }
        public virtual Poste? Poste { get; set; }

        [Display(Name = "Département")]
        public int DepartementId { get; set; }
        public virtual Departement? Departement { get; set; }

        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public virtual ICollection<Contrat>? Contrats { get; set; }
        public virtual ICollection<DemandeConge>? DemandesConges { get; set; }
    }
}