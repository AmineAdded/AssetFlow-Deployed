// src/Backend/AssetFlow.WebAPI/Controllers/AgentController.cs
using AssetFlow.Application.DTOs.AgentDtos;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/agent")]
    [Authorize]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agent;

        public AgentController(IAgentService agent)
        {
            _agent = agent;
        }

        /// <summary>Alertes initiales à l'ouverture du chat</summary>
        [HttpGet("alerts")]
        public async Task<IActionResult> GetInitialAlerts()
        {
            var result = await _agent.GetInitialAlertsAsync();
            return Ok(result);
        }

        /// <summary>Envoyer un message à l'agent</summary>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AgentChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message vide.");

            var result = await _agent.ProcessMessageAsync(request);
            return Ok(result);
        }

        /// <summary>Approuver ou refuser une action proposée par l'agent</summary>
        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] AgentApprovalRequest request)
        {
            // Récupérer le nom de l'utilisateur depuis l'en-tête
            if (string.IsNullOrWhiteSpace(request.Utilisateur))
                request.Utilisateur = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Agent IA";

            var result = await _agent.ApproveActionAsync(request);
            return result.Succes ? Ok(result) : BadRequest(result);
        }
    }
}
