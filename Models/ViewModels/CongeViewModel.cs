using System.ComponentModel.DataAnnotations;
using Systeme_RH.Models.Enums; // Pour accéder à StatutConge et TypeConge

namespace Systeme_RH.Models.ViewModels
{
    public class CongeViewModel
    {
        public int Id { get; set; }

        // --- DÉTAILS DE LA DEMANDE ---

        [Required(ErrorMessage = "Le type de congé est requis.")]
        [Display(Name = "Type de congé")]
        public TypeConge Type { get; set; }

        [Required(ErrorMessage = "La date de début est requise.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; }

        [Required(ErrorMessage = "La date de fin est requise.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; }

        [Display(Name = "Durée (Jours)")]
        public int NombreJours { get; set; }

        [Required(ErrorMessage = "Le motif est obligatoire.")]
        [StringLength(500, ErrorMessage = "Le motif ne peut dépasser 500 caractères.")]
        [Display(Name = "Motif / Commentaire")]
        public string Motif { get; set; } = string.Empty;

        // --- STATUT & VALIDATION (Partie RH) ---

        public EtatDemandeConge Statut { get; set; } = EtatDemandeConge.EnAttente;

        [Display(Name = "Date de la demande")]
        public DateTime DateDemande { get; set; }

        [Display(Name = "Date de traitement")]
        public DateTime? DateValidation { get; set; }

        [Display(Name = "Motif du refus")]
        public string? MotifRefus { get; set; } // Rempli par le RH si refusé

        public decimal EmployeSoldeConges { get; set; }

        // --- INFORMATIONS EMPLOYÉ (Celui qui demande) ---

        public int EmployeId { get; set; }

        [Display(Name = "Employé")]
        public string? EmployeNom { get; set; } // Ex: "Ayman Yassine"

        [Display(Name = "Matricule")]
        public string? EmployeMatricule { get; set; }

        [Display(Name = "Département")]
        public string? EmployeDepartement { get; set; }

        // --- INFORMATIONS VALIDATEUR (Le RH qui a validé) ---

        public string? ValidateurNom { get; set; }


        // --- HELPERS POUR L'AFFICHAGE (UI) ---
        // Ces propriétés calculées permettent d'avoir du code HTML propre

        // Retourne la classe Bootstrap pour la couleur du badge (Vert, Rouge, Jaune)
        public string StatutBadgeClass => Statut switch
        {
            EtatDemandeConge.EnAttente => "bg-warning text-dark", // Jaune
            EtatDemandeConge.Approuve => "bg-success",           // Vert
            EtatDemandeConge.Refuse => "bg-danger",             // Rouge
            EtatDemandeConge.Annule => "bg-secondary",         // Gris
            _ => "bg-light text-dark"
        };

        // Affiche un texte propre pour le type de congé (ex: "Congé Maladie" au lieu de "Maladie")
        public string TypeLibelle => Type switch
        {
            TypeConge.Paye => "Congé Payé (Annuel)",
            TypeConge.Maladie => "Maladie",
            TypeConge.SansSolde => "Sans Solde",
            TypeConge.Maternite => "Maternité",
            TypeConge.Paternite => "Paternité",
            _ => Type.ToString()
        };
    }
}