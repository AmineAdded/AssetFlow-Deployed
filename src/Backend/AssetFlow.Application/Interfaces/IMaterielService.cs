// ============================================================
// AssetFlow.Application / Interfaces / IMaterielService.cs
// Contrat de service pour la gestion du matériel
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    /// <summary>
    /// Service applicatif gérant le CRUD du matériel/stock
    /// </summary>
    public interface IMaterielService
    {
        /// <summary>Récupère tous les matériels (liste complète)</summary>
        Task<IEnumerable<MaterielDto>> GetAllAsync();

        /// <summary>Récupère un matériel par son identifiant</summary>
        Task<MaterielDto?> GetByIdAsync(int id);

        /// <summary>Filtre les matériels par catégorie et/ou terme de recherche</summary>
        Task<IEnumerable<MaterielDto>> SearchAsync(string? terme, string? categorie, string? etat);

        /// <summary>Crée un nouveau matériel et retourne son id</summary>
        Task<MaterielResultDto> CreerAsync(CreerMaterielDto dto);

        /// <summary>Met à jour un matériel existant</summary>
        Task<MaterielResultDto> ModifierAsync(ModifierMaterielDto dto);

        /// <summary>Supprime un matériel par son id</summary>
        Task<MaterielResultDto> SupprimerAsync(int id);

        /// <summary>Retourne le nombre total d'articles, en stock, en alerte et en rupture</summary>
        Task<MaterielStatsDto> GetStatsAsync();
    }

    /// <summary>KPIs synthétiques du stock</summary>
    public class MaterielStatsDto
    {
        public int TotalArticles    { get; set; }
        public int EnStock          { get; set; }   // Disponible
        public int AlerteSeuil      { get; set; }   // Quantité proche ou égale à QuantiteMin
        public int RuptureCritique  { get; set; }   // EnRupture
    }
}