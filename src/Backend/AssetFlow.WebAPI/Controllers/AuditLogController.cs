using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize(Policy = "AdminOnly")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _svc;
        public AuditLogController(IAuditLogService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] AuditLogQueryDto query)
            => Ok(await _svc.GetLogsAsync(query));

        // NOUVEAU — stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
            => Ok(await _svc.GetStatsAsync());

        // NOUVEAU — supprimer avant une date
        [HttpDelete("avant-date")]
        public async Task<IActionResult> SupprimerAvantDate([FromQuery] DateTime date)
        {
            var count = await _svc.SupprimerAvantDateAsync(date);
            return Ok(new { supprimés = count, message = $"{count} entrées supprimées." });
        }

        // NOUVEAU — supprimer par catégorie
        [HttpDelete("par-categorie")]
        public async Task<IActionResult> SupprimerParCategorie([FromQuery] string categorie)
        {
            var count = await _svc.SupprimerParCategorieAsync(categorie);
            return Ok(new { supprimés = count, message = $"{count} entrées supprimées." });
        }

        // NOUVEAU — tout supprimer
        [HttpDelete("tout")]
        public async Task<IActionResult> SupprimerTout()
        {
            var count = await _svc.SupprimerToutAsync();
            return Ok(new { supprimés = count, message = $"{count} entrées supprimées." });
        }
    }
}