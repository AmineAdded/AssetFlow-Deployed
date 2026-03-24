// ============================================================
// AssetFlow.Application / Interfaces / ICommentaireService.cs
// MISE À JOUR : ajout SupprimerCommentaireAsync
// ============================================================

using AssetFlow.Application.DTOs;

namespace AssetFlow.Application.Interfaces
{
    public interface ICommentaireService
    {
        Task<CommentaireResultDto> AjouterCommentaireAsync(CreerCommentaireDto dto);
        Task<List<CommentaireDto>> GetCommentairesMaterielAsync(int materielId, int userId);
        Task<CommentaireResultDto> SupprimerCommentaireAsync(int commentaireId, int utilisateurId);
    }
}