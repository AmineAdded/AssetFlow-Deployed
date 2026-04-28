// src/Backend/AssetFlow.Application/DTOs/AgentDtos/AgentDTOs.cs
namespace AssetFlow.Application.DTOs.AgentDtos
{
    // ── Message envoyé par le user ──
    public class AgentChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<AgentChatHistory> History { get; set; } = new();
    }

    public class AgentChatHistory
    {
        public string Role    { get; set; } = string.Empty; // "user" | "assistant"
        public string Content { get; set; } = string.Empty;
    }

    // ── Réponse de l'agent ──
    public class AgentChatResponse
    {
        public string AgentUsed        { get; set; } = string.Empty; // "web" | "db" | "orchestrator"
        public string Message          { get; set; } = string.Empty;
        public string? RawData         { get; set; }
        public AgentAction? Action     { get; set; } // si une action est proposée
        public List<AlerteStock> Alertes { get; set; } = new();
    }

    // ── Action proposée par l'agent (à approuver par l'user) ──
    public class AgentAction
    {
        public string Type { get; set; } = string.Empty; // "add_materiel" | "add_commande" | "add_article"
        public string Label { get; set; } = string.Empty;
        
        // Formulaire pré-rempli pour approbation
        public AgentMaterielProposal? MaterielProposal   { get; set; }
        public AgentCommandeProposal? CommandeProposal   { get; set; }
        public AgentArticleProposal?  ArticleProposal    { get; set; }
    }

    // ── Formulaires de propositions ──
    public class AgentMaterielProposal
    {
        public string  Reference     { get; set; } = string.Empty;
        public string  Designation   { get; set; } = string.Empty;
        public string? Description   { get; set; }
        public string  Categorie     { get; set; } = string.Empty;
        public int     QuantiteStock { get; set; }
        public int     QuantiteMin   { get; set; }
        public string  Unite         { get; set; } = "pièce";
        public string? Emplacement   { get; set; }
        
        // Commande associée (optionnel)
        public AgentCommandeProposal? Commande { get; set; }
    }

    public class AgentCommandeProposal
    {
        public string    NumeroCommande      { get; set; } = string.Empty;
        public int       MaterielId          { get; set; }
        public string    NomMateriel         { get; set; } = string.Empty;
        public int       FournisseurId       { get; set; }
        public string    NomFournisseur      { get; set; } = string.Empty;
        public int       QuantiteAchetee     { get; set; } = 1;
        public DateTime  DateAchat           { get; set; } = DateTime.UtcNow;
        public DateTime? DateLivraison       { get; set; }
        public DateTime? DateFinGarantie     { get; set; }
        public List<string?> NumerosSerie    { get; set; } = new();
    }

    public class AgentArticleProposal
    {
        public int     MaterielId  { get; set; }
        public string  NomMateriel { get; set; } = string.Empty;
        public int     CommandeId  { get; set; }
        public string? NumeroSerie { get; set; }
    }

    // ── Alerte de stock ──
    public class AlerteStock
    {
        public int    MaterielId   { get; set; }
        public string Reference    { get; set; } = string.Empty;
        public string Designation  { get; set; } = string.Empty;
        public int    QuantiteStock { get; set; }
        public int    QuantiteMin  { get; set; }
        public string Categorie    { get; set; } = string.Empty;
        public AgentMaterielProposal? Proposition { get; set; }
    }

    // ── Approbation d'une action ──
    public class AgentApprovalRequest
    {
        public string ActionType   { get; set; } = string.Empty;
        public bool   Approved     { get; set; }
        public string Utilisateur  { get; set; } = string.Empty;
        
        public AgentMaterielProposal? MaterielProposal   { get; set; }
        public AgentCommandeProposal? CommandeProposal   { get; set; }
        public AgentArticleProposal?  ArticleProposal    { get; set; }
    }

    public class AgentApprovalResponse
    {
        public bool   Succes  { get; set; }
        public string Message { get; set; } = string.Empty;
        public int?   Id      { get; set; }
    }
}