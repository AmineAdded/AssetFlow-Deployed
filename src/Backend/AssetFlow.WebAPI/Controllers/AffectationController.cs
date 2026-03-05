// ============================================================
// AssetFlow.WebAPI / Controllers / AffectationController.cs
// Endpoints pour la gestion des affectations de matériel
// Accès : rôle IT uniquement
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/affectation")]
    [Authorize(Policy = "ITOnly")]
    public class AffectationController : ControllerBase
    {
        private readonly IAffectationService _svc;

        public AffectationController(IAffectationService svc) => _svc = svc;

        /// <summary>
        /// GET api/affectation/utilisateurs?search=...
        /// Retourne la liste des employés disponibles (filtrée si search fourni)
        /// </summary>
        [HttpGet("utilisateurs")]
        public async Task<IActionResult> GetUtilisateurs([FromQuery] string? search = null)
        {
            var result = await _svc.GetUtilisateursDisponiblesAsync(search);
            return Ok(result);
        }

        /// <summary>
        /// GET api/affectation/materiels?search=...
        /// Retourne les matériels ayant au moins un article disponible
        /// </summary>
        [HttpGet("materiels")]
        public async Task<IActionResult> GetMateriels([FromQuery] string? search = null)
        {
            var result = await _svc.GetMaterielsDisponiblesAsync(search);
            return Ok(result);
        }

        /// <summary>
        /// POST api/affectation
        /// Crée une nouvelle affectation
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreerAffectation([FromBody] CreerAffectationDto dto)
        {
            if (dto.MaterielId <= 0 || dto.UtilisateurId <= 0)
                return BadRequest("MaterielId et UtilisateurId sont requis.");

            var result = await _svc.CreerAffectationAsync(dto);

            return result.Succes
                ? Ok(result)
                : BadRequest(result);
        }
    }
}