namespace AssetFlow.BlazorUI.DTOs
{
    public class GraphNodeDto
    {
        public string Id      { get; set; } = string.Empty;
        public string Type    { get; set; } = string.Empty;
        public string Label   { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public string? Status { get; set; }
        public int    Weight  { get; set; } = 1;
    }
}