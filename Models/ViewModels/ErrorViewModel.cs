namespace Systeme_RH.Models
{
    public class ErrorViewModel
    {
        // Identifiant unique de la requête pour le débogage technique
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Ajout personnalisé : Pour afficher un message clair à l'utilisateur
        public string? Message { get; set; }
    }
}