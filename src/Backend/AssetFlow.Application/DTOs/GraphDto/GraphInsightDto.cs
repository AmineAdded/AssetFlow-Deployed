namespace AssetFlow.Application.DTOs
{
    /// <summary>Insight IA affiché dans le panneau droit</summary>
    public class GraphInsightDto
    {
        public string Type     { get; set; } = "info";      // "warning" | "correlation" | "recommendation" | "info"
        public string Title    { get; set; } = string.Empty;
        public string Message  { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}