using Systeme_RH.Models.Enums;

namespace Systeme_RH.Models.ViewModels
{
    public class CongeRapportViewModel
    {
        public int TotalDemandes { get; set; }
        public int EnAttente { get; set; }
        public int Approuvees { get; set; }
        public int Refusees { get; set; }
        public int Annulees { get; set; }

        public Dictionary<TypeConge, int> ParType { get; set; } = new();
        public Dictionary<string, int> ParDepartement { get; set; } = new();
    }
}
