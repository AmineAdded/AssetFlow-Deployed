// src/Backend/AssetFlow.Application/Interfaces/IAgentService.cs
using AssetFlow.Application.DTOs.AgentDtos;

namespace AssetFlow.Application.Interfaces
{
    public interface IAgentService
    {
        Task<AgentChatResponse> ProcessMessageAsync(AgentChatRequest request);
        Task<AgentChatResponse> GetInitialAlertsAsync();
        Task<AgentApprovalResponse> ApproveActionAsync(AgentApprovalRequest request);
    }

    public interface IWebSearchAgentService
    {
        /// <param name="query">Requête de l'utilisateur</param>
        /// <param name="history">Historique de la conversation (optionnel)</param>
        Task<string> SearchAsync(string query,
            List<AssetFlow.Application.DTOs.AgentDtos.AgentChatHistory>? history = null);
    }

    public interface IDatabaseAgentService
    {
        Task<List<AssetFlow.Application.DTOs.AgentDtos.AlerteStock>> GetStockAlertsAsync();
 
        /// <param name="question">Question de l'utilisateur</param>
        /// <param name="history">Historique de la conversation (optionnel)</param>
        Task<string> QueryAsync(string question,
            List<AssetFlow.Application.DTOs.AgentDtos.AgentChatHistory>? history = null);
    }

    public interface IOrchestratorAgentService
    {
        /// <param name="userMessage">Message actuel</param>
        /// <param name="history">Historique de la conversation (optionnel)</param>
        Task<string> DetermineAgentAsync(string userMessage,
            List<AssetFlow.Application.DTOs.AgentDtos.AgentChatHistory>? history = null);
 
        Task<AssetFlow.Application.DTOs.AgentDtos.AgentAction?> ExtractActionAsync(string userMessage,
            List<AssetFlow.Application.DTOs.AgentDtos.AgentChatHistory>? history = null);
 
        Task<AssetFlow.Application.DTOs.AgentDtos.AgentMaterielProposal> GenerateMaterielProposalAsync(
            AssetFlow.Application.DTOs.AgentDtos.AlerteStock alerte);
    }
}
