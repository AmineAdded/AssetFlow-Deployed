namespace AssetFlow.Domain.Entities
{
    public class ArticleHistorique
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }
        public ArticleIndividuel Article { get; set; } = null!;

        public TypeEvenementArticle TypeEvenement { get; set; }

        /// <summary>Utilisateur concerné (affectation / retrait), null si non applicable</summary>
        public int? UtilisateurId { get; set; }
        public User? Utilisateur { get; set; }

        public DateTime DateEvenement { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
    }

    public enum TypeEvenementArticle
    {
        Acquisition   = 0,
        Affectation   = 1,
        Retrait       = 2,
        PanneDeclaree = 3,
        Reparation    = 4,
        MiseEnStock   = 5,
        Reforme       = 6
    }
}
