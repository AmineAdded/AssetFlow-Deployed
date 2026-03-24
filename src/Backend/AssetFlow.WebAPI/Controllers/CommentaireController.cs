// ============================================================
// AssetFlow.WebAPI / Controllers / CommentaireController.cs
// MISE À JOUR : ajout DELETE api/commentaire/{id}/{userId}
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentaireController : ControllerBase
    {
        private readonly ICommentaireService _service;

        public CommentaireController(ICommentaireService service)
        {
            _service = service;
        }

        /// <summary>
        /// POST api/commentaire
        /// Enregistre un commentaire sur un matériel
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AjouterCommentaire([FromBody] CreerCommentaireDto dto)
        {
            if (dto.MaterielId <= 0 || dto.UtilisateurId <= 0)
                return BadRequest("Données invalides.");

            var result = await _service.AjouterCommentaireAsync(dto);
            if (!result.Succes) return BadRequest(result.Message);

            return Ok(result);
        }

        /// <summary>
        /// GET api/commentaire/materiel/{materielId}/{userId}
        /// Récupère les commentaires d'un utilisateur pour un matériel
        /// </summary>
        [HttpGet("materiel/{materielId}/{userId}")]
        public async Task<IActionResult> GetCommentaires(int materielId, int userId)
        {
            if (materielId <= 0) return BadRequest("ID matériel invalide.");
            var commentaires = await _service.GetCommentairesMaterielAsync(materielId, userId);
            return Ok(commentaires);
        }

        /// <summary>
        /// DELETE api/commentaire/{commentaireId}/{utilisateurId}
        /// Supprime un commentaire (seulement par son auteur)
        /// </summary>
        [HttpDelete("{commentaireId}/{utilisateurId}")]
        public async Task<IActionResult> SupprimerCommentaire(int commentaireId, int utilisateurId)
        {
            if (commentaireId <= 0 || utilisateurId <= 0)
                return BadRequest("Données invalides.");

            var result = await _service.SupprimerCommentaireAsync(commentaireId, utilisateurId);
            if (!result.Succes) return BadRequest(result.Message);

            return Ok(result);
        }
    }
}