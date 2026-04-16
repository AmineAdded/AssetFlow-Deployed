using AssetFlow.Application.DTOs;
using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Interfaces
{
    public interface IArticleBiographieService
    {
        /// <summary>Retourne la biographie complète d'un article (unité physique)</summary>
        Task<ArticleBiographieDto?> GetBiographieAsync(int articleId);

        /// <summary>Retourne tous les matériels avec leurs articles pour le sélecteur</summary>
        Task<List<MaterielAvecArticlesDto>> GetMaterielsAvecArticlesAsync();

        /// <summary>Enregistre un événement dans l'historique d'un article</summary>
        Task AjouterEvenementAsync(int articleId, TypeEvenementArticle typeEvenement, int? utilisateurId, string? description);
    }
}
