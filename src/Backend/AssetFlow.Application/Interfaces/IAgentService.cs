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
        Task<string> SearchAsync(string query);
    }

    public interface IDatabaseAgentService
    {
        Task<string> QueryAsync(string question);
        Task<List<AlerteStock>> GetStockAlertsAsync();
    }

    public interface IOrchestratorAgentService
    {
        Task<string> DetermineAgentAsync(string userMessage);
        Task<AgentAction?> ExtractActionAsync(string userMessage);
        Task<AgentMaterielProposal> GenerateMaterielProposalAsync(AlerteStock alerte);
    }
}
