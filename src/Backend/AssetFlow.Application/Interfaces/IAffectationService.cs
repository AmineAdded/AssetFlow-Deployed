// ============================================================
// AssetFlow.Application / Interfaces / IAffectationService.cs
// Interface du service d'affectation de matériel
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    public interface IAffectationService
    {
        /// <summary>
        /// Récupère tous les utilisateurs (rôle Employe) disponibles
        /// </summary>
        Task<List<UtilisateurDisponibleDto>> GetUtilisateursDisponiblesAsync(string? search = null);

        /// <summary>
        /// Récupère tous les matériels ayant au moins un article disponible,
        /// avec la liste de leurs articles disponibles
        /// </summary>
        Task<List<MaterielDisponibleDto>> GetMaterielsDisponiblesAsync(string? search = null);

        /// <summary>
        /// Crée une affectation : lie les articles sélectionnés à l'utilisateur
        /// </summary>
        Task<AffectationResultDto> CreerAffectationAsync(CreerAffectationDto dto);
    }
}