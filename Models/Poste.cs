using System.ComponentModel.DataAnnotations;

namespace Systeme_RH.Models
{
    public class Poste
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Titre { get; set; }
        public string? Description { get; set; }
        public decimal Salaire { get; set; }

        public int DepartementId { get; set; }
        public virtual Departement? Departement { get; set; }
        public virtual ICollection<Employe>? Employes { get; set; }
    }

}
