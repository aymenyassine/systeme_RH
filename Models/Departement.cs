using System.ComponentModel.DataAnnotations;

namespace Systeme_RH.Models
{
    public class Departement
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du département est requis")]
        public string? Nom { get; set; } // Ex: IT, RH, Finance

        public string? Description { get; set; }

        // Relation : Un département a plusieurs postes
        public virtual ICollection<Poste>? Postes { get; set; }
        public virtual ICollection<Employe>? Employes { get; set; }
    }

}