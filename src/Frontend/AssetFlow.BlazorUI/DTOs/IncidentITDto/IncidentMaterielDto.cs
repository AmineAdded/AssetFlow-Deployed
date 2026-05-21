namespace AssetFlow.BlazorUI.DTOs
{
    public class IncidentMaterielDto
    {
        public int    MaterielId         { get; set; }
        public int    AffectationId      { get; set; }
        public string Designation        { get; set; } = string.Empty;
        public string Reference          { get; set; } = string.Empty;
        public string? ImageUrl          { get; set; }
        public string Categorie          { get; set; } = string.Empty;
        public int    NbIncidentsActifs  { get; set; }
        public List<IncidentArticleDto> Articles { get; set; } = new();
        public DateTime DateAffectation   { get; set; }
        public int      QuantiteAffectee  { get; set; }
    }
}