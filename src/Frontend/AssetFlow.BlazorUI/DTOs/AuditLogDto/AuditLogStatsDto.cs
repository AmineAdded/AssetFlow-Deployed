namespace AssetFlow.BlazorUI.DTOs
{
    public class AuditLogStatsDto
    {
        public int   TotalEntrees        { get; set; }
        public int   EntreesAujourdhui   { get; set; }
        public int   EntreesCeMois       { get; set; }
        public DateTime? PlusAncienneEntree { get; set; }
        public Dictionary<string, int> ParCategorie { get; set; } = new();
    }
}