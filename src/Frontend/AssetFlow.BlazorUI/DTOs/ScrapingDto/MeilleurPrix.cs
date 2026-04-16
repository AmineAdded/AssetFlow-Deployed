namespace AssetFlow.BlazorUI.DTOs
{
    public class MeilleurPrix
        {
            public string site { get; set; } = string.Empty;
            public string nom_produit { get; set; } = string.Empty;
            public decimal prix { get; set; }
            public string stock { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
        }
}