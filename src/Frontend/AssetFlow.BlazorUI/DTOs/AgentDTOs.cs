// src/Frontend/AssetFlow.BlazorUI/DTOs/AgentDTOs.cs
using System.Text.Json.Serialization;

namespace AssetFlow.BlazorUI.DTOs
{
    public class AgentChatRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("history")]
        public List<AgentChatHistory> History { get; set; } = new();
    }

    public class AgentChatHistory
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class AgentChatResponse
    {
        [JsonPropertyName("agentUsed")]
        public string AgentUsed { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("action")]
        public AgentAction? Action { get; set; }

        [JsonPropertyName("alertes")]
        public List<AlerteStock> Alertes { get; set; } = new();
    }

    public class AgentAction
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("materielProposal")]
        public AgentMaterielProposal? MaterielProposal { get; set; }

        [JsonPropertyName("commandeProposal")]
        public AgentCommandeProposal? CommandeProposal { get; set; }

        [JsonPropertyName("articleProposal")]
        public AgentArticleProposal? ArticleProposal { get; set; }
    }

    public class AgentMaterielProposal
    {
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("designation")]
        public string Designation { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("categorie")]
        public string Categorie { get; set; } = string.Empty;

        [JsonPropertyName("quantiteStock")]
        public int QuantiteStock { get; set; }

        [JsonPropertyName("quantiteMin")]
        public int QuantiteMin { get; set; }

        [JsonPropertyName("unite")]
        public string Unite { get; set; } = "pièce";

        [JsonPropertyName("emplacement")]
        public string? Emplacement { get; set; }

        [JsonPropertyName("commande")]
        public AgentCommandeProposal? Commande { get; set; }
    }

    public class AgentCommandeProposal
    {
        [JsonPropertyName("numeroCommande")]
        public string NumeroCommande { get; set; } = string.Empty;

        [JsonPropertyName("materielId")]
        public int MaterielId { get; set; }

        [JsonPropertyName("nomMateriel")]
        public string NomMateriel { get; set; } = string.Empty;

        [JsonPropertyName("fournisseurId")]
        public int FournisseurId { get; set; }

        [JsonPropertyName("nomFournisseur")]
        public string NomFournisseur { get; set; } = string.Empty;

        [JsonPropertyName("quantiteAchetee")]
        public int QuantiteAchetee { get; set; } = 1;

        [JsonPropertyName("dateAchat")]
        public DateTime DateAchat { get; set; } = DateTime.Today;

        [JsonPropertyName("dateLivraison")]
        public DateTime? DateLivraison { get; set; }

        [JsonPropertyName("dateFinGarantie")]
        public DateTime? DateFinGarantie { get; set; }

        [JsonPropertyName("numerosSerie")]
        public List<string?> NumerosSerie { get; set; } = new();
    }

    public class AgentArticleProposal
    {
        [JsonPropertyName("materielId")]
        public int MaterielId { get; set; }

        [JsonPropertyName("nomMateriel")]
        public string NomMateriel { get; set; } = string.Empty;

        [JsonPropertyName("commandeId")]
        public int CommandeId { get; set; }

        [JsonPropertyName("numeroSerie")]
        public string? NumeroSerie { get; set; }
    }

    public class AlerteStock
    {
        [JsonPropertyName("materielId")]
        public int MaterielId { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("designation")]
        public string Designation { get; set; } = string.Empty;

        [JsonPropertyName("quantiteStock")]
        public int QuantiteStock { get; set; }

        [JsonPropertyName("quantiteMin")]
        public int QuantiteMin { get; set; }

        [JsonPropertyName("categorie")]
        public string Categorie { get; set; } = string.Empty;

        [JsonPropertyName("proposition")]
        public AgentMaterielProposal? Proposition { get; set; }
    }

    public class AgentApprovalRequest
    {
        [JsonPropertyName("actionType")]
        public string ActionType { get; set; } = string.Empty;

        [JsonPropertyName("approved")]
        public bool Approved { get; set; }

        [JsonPropertyName("utilisateur")]
        public string Utilisateur { get; set; } = string.Empty;

        [JsonPropertyName("materielProposal")]
        public AgentMaterielProposal? MaterielProposal { get; set; }

        [JsonPropertyName("commandeProposal")]
        public AgentCommandeProposal? CommandeProposal { get; set; }

        [JsonPropertyName("articleProposal")]
        public AgentArticleProposal? ArticleProposal { get; set; }
    }

    public class AgentApprovalResponse
    {
        [JsonPropertyName("succes")]
        public bool Succes { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public int? Id { get; set; }
    }
}
