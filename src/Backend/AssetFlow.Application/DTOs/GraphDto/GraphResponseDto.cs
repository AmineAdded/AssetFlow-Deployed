namespace AssetFlow.Application.DTOs
{
    /// <summary>Réponse complète de l'endpoint /api/graph</summary>
    public class GraphResponseDto
    {
        public List<GraphNodeDto>    Nodes    { get; set; } = new();
        public List<GraphLinkDto>    Links    { get; set; } = new();
        public List<GraphInsightDto> Insights { get; set; } = new();
        public GraphStatsDto         Stats    { get; set; } = new();
    }
}