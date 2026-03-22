// ============================================================
// AssetFlow.Application / Interfaces / IStatistiquesITService.cs
// Interface du service de statistiques IT
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    public interface IStatistiquesITService
    {
        Task<DashboardITStatsDto> GetDashboardITAsync();
    }
}
