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
}