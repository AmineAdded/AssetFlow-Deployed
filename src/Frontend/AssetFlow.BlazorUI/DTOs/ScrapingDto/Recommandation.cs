namespace AssetFlow.BlazorUI.DTOs
{
    public class Recommandation
        {
            public string? site { get; set; }
            public decimal? prix { get; set; }
            public string? url { get; set; }
            public string message { get; set; } = string.Empty;
        }
}