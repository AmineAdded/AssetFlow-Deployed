// ============================================================
// AssetFlow.Application / Interfaces / IRedisOffreService.cs
// ============================================================

namespace AssetFlow.Application.Interfaces
{
    public interface IRedisOffreService
    {
        /// <summary>Sauvegarde la sélection d'une offre dans Redis.</summary>
        Task SaveOffreSelectionAsync(string key, string jsonValue, TimeSpan? expiry = null);

        /// <summary>Récupère une sélection depuis Redis.</summary>
        Task<string?> GetOffreSelectionAsync(string key);

        /// <summary>Supprime une sélection depuis Redis.</summary>
        Task DeleteOffreSelectionAsync(string key);
    }
}