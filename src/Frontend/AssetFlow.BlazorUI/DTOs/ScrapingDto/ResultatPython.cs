namespace AssetFlow.BlazorUI.DTOs
{
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
}