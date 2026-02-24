// ============================================================
// AssetFlow.WebAPI / Controllers / MaterielController.cs
// API REST pour la gestion du matériel/stock
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    /// <summary>
    /// Contrôleur REST exposant les opérations CRUD sur le matériel.
    /// Route de base : /api/materiel
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MaterielController : ControllerBase
    {
        private readonly IMaterielService _service;

        public MaterielController(IMaterielService service) => _service = service;

        // ── GET /api/materiel ─────────────────────────────────────
        /// <summary>Retourne la liste complète du matériel</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MaterielDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ── GET /api/materiel/{id} ────────────────────────────────
        /// <summary>Retourne un matériel par son identifiant</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MaterielDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        // ── GET /api/materiel/search?terme=&categorie=&etat= ─────
        /// <summary>Recherche avec filtres optionnels</summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<MaterielDto>), 200)]
        public async Task<IActionResult> Search(
            [FromQuery] string? terme,
            [FromQuery] string? categorie,
            [FromQuery] string? etat)
        {
            var result = await _service.SearchAsync(terme, categorie, etat);
            return Ok(result);
        }

        // ── GET /api/materiel/stats ───────────────────────────────
        /// <summary>Retourne les KPIs du stock (pour les cards)</summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(MaterielStatsDto), 200)]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _service.GetStatsAsync();
            return Ok(stats);
        }

        // ── POST /api/materiel ────────────────────────────────────
        /// <summary>Crée un nouveau matériel</summary>
        [HttpPost]
        [ProducesResponseType(typeof(MaterielResultDto), 200)]
        [ProducesResponseType(typeof(MaterielResultDto), 400)]
        public async Task<IActionResult> Creer([FromBody] CreerMaterielDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreerAsync(dto);
            return result.Succes ? Ok(result) : BadRequest(result);
        }

        // ── PUT /api/materiel/{id} ────────────────────────────────
        /// <summary>Met à jour un matériel existant</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(MaterielResultDto), 200)]
        [ProducesResponseType(typeof(MaterielResultDto), 400)]
        public async Task<IActionResult> Modifier(int id, [FromBody] ModifierMaterielDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new MaterielResultDto { Succes = false, Message = "ID incohérent." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.ModifierAsync(dto);
            return result.Succes ? Ok(result) : BadRequest(result);
        }

        // ── DELETE /api/materiel/{id} ─────────────────────────────
        /// <summary>Supprime un matériel</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(MaterielResultDto), 200)]
        [ProducesResponseType(typeof(MaterielResultDto), 400)]
        public async Task<IActionResult> Supprimer(int id)
        {
            var result = await _service.SupprimerAsync(id);
            return result.Succes ? Ok(result) : BadRequest(result);
        }
    }
}