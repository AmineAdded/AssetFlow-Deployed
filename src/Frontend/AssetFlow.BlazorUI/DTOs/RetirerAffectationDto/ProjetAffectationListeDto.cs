namespace AssetFlow.BlazorUI.DTOs
{
    public class ProjetAffectationListeDto
    {
        public int    Id          { get; set; }
        public string Nom         { get; set; } = string.Empty;
        public string Statut      { get; set; } = string.Empty;
        public string Priorite    { get; set; } = string.Empty;
        public string? Responsable { get; set; }
        public int    NbAffectationsActives { get; set; }
    }
}