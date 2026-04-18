namespace AssetFlow.Application.DTOs
{
    public class EvenementArticleDto
    {
        public int Id { get; set; }
        public string TypeEvenement { get; set; } = string.Empty;
        public DateTime DateEvenement { get; set; }
        public string? UtilisateurNom { get; set; }
        public string? Description { get; set; }

        /// <summary>Durée depuis l'événement précédent (en jours), null pour le premier</summary>
        public int? DureeDepuisPrecedent { get; set; }
    }
}