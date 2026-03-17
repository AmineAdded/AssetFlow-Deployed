// ============================================================
// AssetFlow.WebAPI / Controllers / OffreSelectionController.cs
// POST api/offre-selection/confirm   → sauvegarde dans Redis
// GET  api/offre-selection/{userId}  → récupère les sélections
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/offre-selection")]
    [Authorize(Policy = "ITOnly")]
    public class OffreSelectionController : ControllerBase
    {
        private readonly IRedisOffreService _redis;

        public OffreSelectionController(IRedisOffreService redis)
        {
            _redis = redis;
        }

        // POST api/offre-selection/confirm
        // Body : { nomPdf, contenu, userId }
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] OffreSelectionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                return BadRequest("userId est requis.");

            if (string.IsNullOrWhiteSpace(dto.NomPdf))
                return BadRequest("nomPdf est requis.");

            // Clé Redis : offre_selection:{userId}:{nomPdf} (sans espaces)
            var safeName = dto.NomPdf.Replace(" ", "_").Replace("/", "_");
            var key      = $"offre_selection:{dto.UserId}:{safeName}";

            var json = JsonSerializer.Serialize(new
            {
                nomPdf  = dto.NomPdf,
                contenu = dto.Contenu,
                user_id = dto.UserId,
                savedAt = DateTime.UtcNow
            });

             // Sauvegarde sélection
            await _redis.SaveOffreSelectionAsync(key, json, TimeSpan.FromDays(30));

            // Supprimer le cache OCR de cette offre
            if (dto.OffreId != Guid.Empty)
                await _redis.DeleteOffreSelectionAsync($"ocr_cache:{dto.OffreId}");

            return Ok(new { success = true, key });
        }

        // GET api/offre-selection/{userId}
        // Récupère toutes les sélections d'un utilisateur (pour usage futur)
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            // Pour une liste complète, on pourrait utiliser SCAN sur Redis.
            // Ici on retourne juste un message de succès pour l'instant.
            return Ok(new { message = $"Sélections de {userId} disponibles dans Redis." });
        }
    }
}