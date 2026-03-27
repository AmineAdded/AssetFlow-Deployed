using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.WebAPI.Controllers
{
   [ApiController]
    [Route("api/chat-offre")]
    [Authorize(Policy = "ITOrAdmin")]
    public class ChatOffreController : ControllerBase
    {
        private readonly IRedisOffreService _redis;
        private readonly IConfiguration    _config;
        private readonly HttpClient        _http;

        public ChatOffreController(IRedisOffreService redis, IConfiguration config, HttpClient http)
        {
            _redis  = redis;
            _config = config;
            _http   = http;
        }

        // POST api/chat-offre/send
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ChatOffreRequestDto dto)
        {
            var historyKey = $"chat_offre:{dto.UserId}:{dto.IdDemande}";

            // 1. Charger historique depuis Redis
            var historyJson = await _redis.GetOffreSelectionAsync(historyKey);
            var history = string.IsNullOrEmpty(historyJson)
                ? new List<ChatMessageDto>()
                : JsonSerializer.Deserialize<List<ChatMessageDto>>(historyJson) ?? new();

            // 2. Construire contexte OCR
            var contexte = string.Join("\n", dto.Offres.Select(o =>
                $"- {o.NomFichier}: Prix={o.PrixTotal ?? "N/A"}, Délai={o.DelaiLivraison ?? "N/A"}, Garantie={o.Garantie ?? "N/A"}, Frais={o.FraisLivraison ?? "N/A"}"));

            var systemPrompt = $@"Tu es un assistant d'aide à la décision pour la sélection d'offres fournisseurs.

Voici les offres disponibles :
{contexte}

RÈGLES DE FORMATAGE OBLIGATOIRES :
- Quand tu mentionnes un nom de fichier PDF, écris-le toujours en MAJUSCULES entre guillemets doubles, ex : ""FACTURE.PDF""
- Structure toujours ta réponse avec des sections claires séparées par des sauts de ligne
- Utilise des tirets pour les listes
- Sois concis : maximum 5 lignes par réponse
- Quand tu recommandes une offre, écris son nom entre [[ ]] ex : [[facture.pdf]] (nom exact, sensible à la casse)
- Ne recommande qu'une seule offre à la fois

Réponds en français.";

            // 3. Construire messages pour DeepSeek
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };
            foreach (var h in history.TakeLast(10)) // garder les 10 derniers
                messages.Add(new { role = h.Role, content = h.Content });
            messages.Add(new { role = "user", content = dto.Message });

            // 4. Appel DeepSeek
           var payload = new
            {
                model = "meta-llama/Llama-4-Scout-17B-16E-Instruct",  // même modèle que l'OCR
                messages,
                temperature = 0.7,
                max_tokens = 500,
                stream = false
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config["GroqApiKey"]);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseJson);
            var assistantReply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            // 5. Sauvegarder dans historique Redis (24h)
            history.Add(new ChatMessageDto { Role = "user",      Content = dto.Message,      SentAt = DateTime.UtcNow });
            history.Add(new ChatMessageDto { Role = "assistant", Content = assistantReply,   SentAt = DateTime.UtcNow });
            await _redis.SaveOffreSelectionAsync(historyKey, JsonSerializer.Serialize(history), TimeSpan.FromHours(24));


            // 6. Détecter offre recommandée [[nom.pdf]]
            string? recommended = null;
            var match = System.Text.RegularExpressions.Regex.Match(assistantReply, @"\[\[(.+?)\]\]");
            if (match.Success) recommended = match.Groups[1].Value;

            // Après la sauvegarde de l'historique, sauvegarder aussi la recommandation séparément
            if (!string.IsNullOrEmpty(recommended))
            {
                var recKey = $"chat_offre_rec:{dto.UserId}:{dto.IdDemande}";
                await _redis.SaveOffreSelectionAsync(recKey, recommended, TimeSpan.FromHours(24));
            }

            return Ok(new { reply = assistantReply, recommendedOffre = recommended });
        }

        // GET api/chat-offre/history/{userId}/{idDemande}
        [HttpGet("history/{userId}/{idDemande:int}")]
        public async Task<IActionResult> GetHistory(string userId, int idDemande)
        {
            var key = $"chat_offre:{userId}:{idDemande}";
            var json = await _redis.GetOffreSelectionAsync(key);
            if (string.IsNullOrEmpty(json)) return Ok(new List<ChatMessageDto>());
            return Ok(JsonSerializer.Deserialize<List<ChatMessageDto>>(json));
        }

        // DELETE api/chat-offre/history/{userId}/{idDemande}
        [HttpDelete("history/{userId}/{idDemande:int}")]
        public async Task<IActionResult> DeleteHistory(string userId, int idDemande)
        {
            await _redis.DeleteOffreSelectionAsync($"chat_offre:{userId}:{idDemande}");
            return Ok();
        }
        // GET api/chat-offre/recommendation/{userId}/{idDemande}
        [HttpGet("recommendation/{userId}/{idDemande:int}")]
        public async Task<IActionResult> GetRecommendation(string userId, int idDemande)
        {
            var key = $"chat_offre_rec:{userId}:{idDemande}";
            var rec = await _redis.GetOffreSelectionAsync(key);
            return Ok(new { recommendedOffre = rec });
        }
    } 
}
