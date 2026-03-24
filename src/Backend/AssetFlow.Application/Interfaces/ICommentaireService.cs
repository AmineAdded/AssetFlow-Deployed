// ============================================================
// AssetFlow.Application / Interfaces / ICommentaireService.cs
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    public interface ICommentaireService
    {
        /// <summary>Ajoute un commentaire sur un matériel</summary>
        Task<CommentaireResultDto> AjouterCommentaireAsync(CreerCommentaireDto dto);

        /// <summary>Récupère tous les commentaires d'un matériel</summary>
        Task<List<CommentaireDto>> GetCommentairesMaterielAsync(int materielId);
    }
}
