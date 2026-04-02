namespace AssetFlow.BlazorUI.DTOs
{
    public class IncidentEmployeDto
    {
        public int    UtilisateurId     { get; set; }
        public string FullName          { get; set; } = string.Empty;
        public string Role        { get; set; } = string.Empty;
        public string Initials          { get; set; } = string.Empty;
        public int    NbIncidentsActifs { get; set; }
    }

    public class IncidentItemDto
    {
        public int      Id                      { get; set; }
        public int      AffectationId           { get; set; }
        public string   NumeroIncident          { get; set; } = string.Empty;
        public string   TypeIncident            { get; set; } = string.Empty;
        public int      Urgence                 { get; set; }
        public string   UrgenceLabel            { get; set; } = string.Empty;
        public string   Description             { get; set; } = string.Empty;
        public DateTime DateIncident            { get; set; }
        public string   Statut                  { get; set; } = string.Empty;
        public string   StatutLabel             { get; set; } = string.Empty;
        public DateTime? DateResolution         { get; set; }
        public string?  CommentairesResolution  { get; set; }
    }

    public class IncidentArticleDto
    {
        public int    ArticleId    { get; set; }
        public string NumeroSerie  { get; set; } = string.Empty;
        public string EtatArticle  { get; set; } = string.Empty;
        public List<IncidentItemDto> Incidents { get; set; } = new();
    }

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
    }
}