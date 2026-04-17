using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class GraphController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        /// <summary>
        /// Retourne le graphe complet : nœuds, liens et insights.
        /// Utilisé par la page "Mémoire intelligente" (Admin uniquement).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AssetFlow.Application.DTOs.GraphResponseDto), 200)]
        public async Task<IActionResult> GetGraph()
        {
            try
            {
                var result = await _graphService.GetGraphAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la génération du graphe.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Retourne un insight détaillé pour un nœud spécifique (clic sur nœud).
        /// </summary>
        [HttpGet("insight/{nodeId}")]
        public async Task<IActionResult> GetNodeInsight(string nodeId)
        {
            var insight = await _graphService.GetInsightForNodeAsync(nodeId);
            if (insight == null) return NotFound();
            return Ok(insight);
        }
    }
}