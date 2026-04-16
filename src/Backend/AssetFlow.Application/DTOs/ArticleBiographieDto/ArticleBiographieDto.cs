namespace AssetFlow.Application.DTOs
{
    public class ArticleBiographieDto
    {
        // ── Identité de l'article ──
        public int ArticleId { get; set; }
        public string NumeroSerie { get; set; } = string.Empty;

        // Champs réels de Materiel
        public string MaterielReference { get; set; } = string.Empty;
        public string MaterielDesignation { get; set; } = string.Empty;
        public string MaterielCategorie { get; set; } = string.Empty;

        public DateTime DateAcquisition { get; set; }   // date de la commande liée
        public string Statut { get; set; } = string.Empty;
        public string Etat { get; set; } = string.Empty;

        // ── Statistiques calculées ──
        public int AgeTotalJours { get; set; }
        public int NombrePersonnes { get; set; }
        public int NombreIncidents { get; set; }
        public int NombreReparations { get; set; }
        public int JoursEnStock { get; set; }

        // ── Affectation actuelle ──
        public string? AffectationActuelle { get; set; }

        // ── Timeline des événements ──
        public List<EvenementArticleDto> Historique { get; set; } = new();
    }
}