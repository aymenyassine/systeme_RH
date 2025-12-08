using Systeme_RH.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Systeme_RH.Models
{
    public class DemandeConge
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }

        public DateTime DateDemande { get; set; } = DateTime.Now;

        // Nombre de jours calculés (excluant les weekends si besoin de logique complexe plus tard)
        public int NombreJours { get; set; }

        [Required]
        public string? Motif { get; set; }

        // Enums
        public TypeConge TypeConge { get; set; }
        public EtatDemandeConge Etat { get; set; } = EtatDemandeConge.EnAttente;

        // Partie RH
        public string? CommentaireRH { get; set; } // Justification si refus
        public DateTime? DateTraitement { get; set; }

        // Relation
        public int EmployeId { get; set; }
        public virtual Employe? Employe { get; set; }
    }
}