using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    public interface IGraphService
    {
        Task<GraphResponseDto> GetGraphAsync();
        Task<GraphInsightDto?> GetInsightForNodeAsync(string nodeId);
    }
}