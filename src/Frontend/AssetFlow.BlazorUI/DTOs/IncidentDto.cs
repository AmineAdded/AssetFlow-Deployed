namespace AssetFlow.BlazorUI.DTOs
{
     public class SignalerIncidentRequestDto
    {
        public int AffectationId { get; set; }
        public int? ArticleId { get; set; }
        public string TypeIncident { get; set; } = string.Empty;
        public int Urgence { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // DTO pour la réponse après signalement
    public class SignalerIncidentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? IncidentId { get; set; }
        public string? NumeroIncident { get; set; }
    }

    // DTO représentant un incident
    public class IncidentDto
    {
        public int Id { get; set; }
        public int AffectationId { get; set; }
        public string NumeroIncident { get; set; } = string.Empty;
        public string TypeIncident { get; set; } = string.Empty;
        public int Urgence { get; set; }
        public string UrgenceLabel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateIncident { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string StatutLabel { get; set; } = string.Empty;
        public DateTime? DateResolution { get; set; }
        public string? CommentairesResolution { get; set; }
        public string MaterielDesignation { get; set; } = string.Empty;
        public string MaterielReference { get; set; } = string.Empty;
    }
}