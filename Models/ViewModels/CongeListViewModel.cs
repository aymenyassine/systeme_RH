using Systeme_RH.Models.Enums;

namespace Systeme_RH.Models.ViewModels
{
    public class CongeListViewModel
    {
        public List<CongeViewModel> Conges { get; set; } = new List<CongeViewModel>();

        // Filtres
        public EtatDemandeConge? StatutFilter { get; set; }
        public TypeConge? TypeFilter { get; set; }
        public int? EmployeIdFilter { get; set; }

        // Statistiques
        public int TotalDemandes { get; set; }
        public int DemandesEnAttente { get; set; }
        public int DemandesValidees { get; set; }
        public int DemandesRefusees { get; set; }
    }
}