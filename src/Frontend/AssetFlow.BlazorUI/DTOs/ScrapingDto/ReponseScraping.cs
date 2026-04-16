namespace AssetFlow.BlazorUI.DTOs
{
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
}