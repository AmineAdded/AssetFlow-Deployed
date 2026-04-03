using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/voice")]
    [Authorize] // JWT requis
    public class VoiceController : ControllerBase
    {
        private readonly IVoiceService _voice;

        public VoiceController(IVoiceService voice) => _voice = voice;

        /// <summary>
        /// Pipeline complet : reçoit audio base64 + rôle,
        /// retourne transcript + intention + paramètres
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] VoiceCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AudioBase64))
                return BadRequest(new { error = "Audio manquant." });

            if (string.IsNullOrWhiteSpace(request.Role))
                return BadRequest(new { error = "Rôle manquant." });

            var result = await _voice.ProcessAsync(request);

            if (!string.IsNullOrEmpty(result.Error))
                return StatusCode(500, result);

            return Ok(result);
        }

        /// <summary>
        /// NLU seul (utile pour tests ou si la transcription est déjà faite)
        /// </summary>
        [HttpPost("parse")]
        public async Task<IActionResult> Parse([FromBody] ParseIntentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Transcript))
                return BadRequest(new { error = "Transcript manquant." });

            var result = await _voice.ParseIntentAsync(request.Transcript, request.Role);
            return Ok(result);
        }
    }
}