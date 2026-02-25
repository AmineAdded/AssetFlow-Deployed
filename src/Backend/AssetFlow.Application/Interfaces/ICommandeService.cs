// ============================================================
// AssetFlow.Application / Interfaces / ICommandeService.cs — v3
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    public interface ICommandeService
    {
        Task<IEnumerable<CommandeDto>>              GetAllAsync();
        Task<IEnumerable<CommandeDto>>              GetByMaterielAsync(int materielId);
        Task<CommandeDto?>                          GetByIdAsync(int id);

        /// <summary>Une ligne par commande (toutes commandes / tous matériels)</summary>
        Task<IEnumerable<LigneCommandeMaterielDto>> GetLignesCommandesAsync();

        /// <summary>Articles individuels d'un matériel (toutes commandes)</summary>
        Task<IEnumerable<ArticleDto>>               GetArticlesByMaterielAsync(int materielId);

        /// <summary>Articles individuels d'une commande précise</summary>
        Task<IEnumerable<ArticleDto>>               GetArticlesByCommandeAsync(int commandeId);

        Task<CommandeReponseDto> CreerAsync(CreerCommandeDto dto);
        Task<CommandeReponseDto> SupprimerAsync(int id);
    }
}