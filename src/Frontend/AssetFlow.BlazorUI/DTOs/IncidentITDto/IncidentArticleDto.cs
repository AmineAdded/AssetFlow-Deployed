namespace AssetFlow.BlazorUI.DTOs
{
    public class IncidentArticleDto
    {
        public int    ArticleId    { get; set; }
        public string NumeroSerie  { get; set; } = string.Empty;
        public string EtatArticle  { get; set; } = string.Empty;
        public List<IncidentItemDto> Incidents { get; set; } = new();
    }
}