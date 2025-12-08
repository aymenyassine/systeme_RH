using System.ComponentModel.DataAnnotations;
using Systeme_RH.Models.Enums; // Assure-toi que ton Enum TypeContrat est accessible

namespace Systeme_RH.Models.ViewModels
{
    public class ContratViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La date de début est requise.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de fin")]
        // Note : La date de fin est optionnelle (null) pour un CDI
        public DateTime? DateFin { get; set; }

        [Required(ErrorMessage = "Le salaire mensuel est requis.")]
        [Range(0, double.MaxValue, ErrorMessage = "Le salaire doit être positif.")]
        [Display(Name = "Salaire Mensuel (DH)")]
        public decimal SalaireMensuel { get; set; }

        [Required(ErrorMessage = "Le type de contrat est requis.")]
        [Display(Name = "Type de contrat")]
        public TypeContrat TypeContrat { get; set; }

        [Display(Name = "Pièce jointe")]
        public string? PieceJointeUrl { get; set; }

        // --- RELATIONS ---

        [Required(ErrorMessage = "L'employé est requis.")]
        public int EmployeId { get; set; }

        // Pour l'affichage (ex: "Contrat de Ahmed Benali")
        [Display(Name = "Employé")]
        public string? EmployeNom { get; set; }

        [Display(Name = "Statut Actif")]
        public bool EstActif { get; set; } = true;
    }
}