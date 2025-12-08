namespace Systeme_RH.Models.Enums
{
   public enum EtatDemandeConge
    {
        EnAttente,      // Par défaut lors de la création
        Approuve,       // Validé par le RH -> Déduit du solde
        Refuse,         // Refusé par le RH
        Annule          // Annulé par l'employé avant validation
    }
}