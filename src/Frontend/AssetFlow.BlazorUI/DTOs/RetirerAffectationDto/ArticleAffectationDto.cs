namespace AssetFlow.BlazorUI.DTOs
{
    public class ArticleAffectationDto
    {
        public int    ArticleId   { get; set; }
        public string NumeroSerie { get; set; } = string.Empty;
        public string Etat        { get; set; } = string.Empty;
    }
}