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
        private readonly IOffreAchatService _offres;
        private readonly IOcrInvoiceService _ocr;

        public OffreSelectionController(
            IRedisOffreService redis,
            IOffreAchatService offres,
            IOcrInvoiceService ocr)
        {
            _redis  = redis;
            _offres = offres;
            _ocr    = ocr;
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] OffreSelectionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId))
                return BadRequest("userId est requis.");
            if (dto.OffreId == Guid.Empty)
                return BadRequest("offreId est requis.");
            if (dto.IdDemande == 0)
                return BadRequest("idDemande est requis.");

            // 1. Mettre EstChoisie = true en SQL
            var success = await _offres.ChoisirOffreAsync(dto.OffreId, dto.IdDemande);
            if (!success)
                return NotFound("Offre introuvable.");

            // 2. Supprimer TOUS les caches OCR des offres de cette demande
            var toutesLesOffres = await _offres.GetByDemandeIdAsync(dto.IdDemande);
            foreach (var offre in toutesLesOffres)
                await _redis.DeleteOffreSelectionAsync($"ocr_cache:{offre.IdOffre}");

            return Ok(new { success = true });
        }
    }
}