using Systeme_RH.Models.Enums;

namespace Systeme_RH.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        // Infos Utilisateur
        public string? NomComplet { get; set; }
        public string? RolePrincipal { get; set; }
        public int? EmployeId { get; set; }

        // --- SECTION EMPLOYÉ (Mes données) ---
        public int MonSoldeConges { get; set; }
        public int MesDemandesEnAttente { get; set; }
        public bool AUnContratActif { get; set; }

        // --- SECTION RH / ADMIN (Statistiques Globales) ---
        public int TotalEmployes { get; set; }
        public int TotalDemandesCongesEnAttente { get; set; }
        public int TotalContratsActifs { get; set; }
        public int TotalPostes { get; set; }

        public int TotalDepartements { get; set; }
    }
}