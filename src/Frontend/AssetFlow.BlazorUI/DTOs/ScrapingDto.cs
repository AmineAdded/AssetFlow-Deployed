namespace AssetFlow.BlazorUI.DTOs
{
     public class ResultatScraping
        {
            public string Site { get; set; } = string.Empty;
            public string NomProduit { get; set; } = string.Empty;
            public decimal Prix { get; set; }
            public bool EnStock { get; set; }
            public string Livraison { get; set; } = "Non précisé";
            public string Garantie { get; set; } = "Non précisée";
            public string Url { get; set; } = string.Empty;
        }

        public class ReponseScraping
        {
            public bool succes { get; set; }
            public string article { get; set; } = string.Empty;
            public string date_recherche { get; set; } = string.Empty;
            public int nombre_resultats { get; set; }
            public List<ResultatPython> resultats { get; set; } = new();
            public MeilleurPrix? meilleur_prix { get; set; }
            public Recommandation? recommandation { get; set; }
        }

        public class ResultatPython
        {
            public string site { get; set; } = string.Empty;
            public string nom_produit { get; set; } = string.Empty;
            public decimal prix { get; set; }
            public string devise { get; set; } = string.Empty;
            public string stock { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string date_scraping { get; set; } = string.Empty;
        }

        public class MeilleurPrix
        {
            public string site { get; set; } = string.Empty;
            public string nom_produit { get; set; } = string.Empty;
            public decimal prix { get; set; }
            public string stock { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
        }

        public class Recommandation
        {
            public string? site { get; set; }
            public decimal? prix { get; set; }
            public string? url { get; set; }
            public string message { get; set; } = string.Empty;
        }
}