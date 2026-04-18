namespace AssetFlow.BlazorUI.DTOs
{
    public class GraphInsightDto
    {
        public string  Type       { get; set; } = "info";
        public string  Title      { get; set; } = string.Empty;
        public string  Message    { get; set; } = string.Empty;
        public string? EntityId   { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}