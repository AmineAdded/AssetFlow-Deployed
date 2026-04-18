namespace AssetFlow.Application.DTOs
{
    public class GraphStatsDto
    {
        public int TotalMateriel   { get; set; }
        public int TotalIncidents  { get; set; }
        public int TotalUsers      { get; set; }
        public int ActiveAnomalies { get; set; }
    }
}