using Systeme_RH.Models;
using Systeme_RH.Models.Enums;

namespace Systeme_RH.Models;
public class Conge
{
    public int Id { get; set; }
    public TypeConge Type { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public int NombreJours { get; set; }
    public EtatDemandeConge Statut { get; set; }
    public string? Motif { get; set; }
    public string? MotifRefus { get; set; }
    public DateTime DateDemande { get; set; }
    public DateTime? DateValidation { get; set; }
    
    public int EmployeId { get; set; }
    public virtual Employe? Employe { get; set; }
    
    public int? ValidePar { get; set; }
    public virtual ApplicationUser? Validateur { get; set; }
}
