using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Systeme_RH.Models.Enums;

namespace Systeme_RH.Models
{
  public class Contrat
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; } // Nullable si CDI

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaireMensuel { get; set; }

        // Utilisation de l'Enum
        public TypeContrat TypeContrat { get; set; }

        public string? PieceJointeUrl { get; set; } // Chemin vers le fichier PDF du contrat

        public bool EstActif { get; set; } = true;

        public int EmployeId { get; set; }
        public virtual Employe? Employe { get; set; }
    }
}
