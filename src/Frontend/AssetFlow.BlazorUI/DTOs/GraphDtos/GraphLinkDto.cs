namespace AssetFlow.BlazorUI.DTOs
{
    public class GraphLinkDto
    {
        public string  Source   { get; set; } = string.Empty;
        public string  Target   { get; set; } = string.Empty;
        public string? Label    { get; set; }
        public double  Strength { get; set; } = 0.5;
    }
}