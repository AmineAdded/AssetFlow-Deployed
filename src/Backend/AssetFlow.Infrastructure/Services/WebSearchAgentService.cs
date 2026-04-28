// src/Backend/AssetFlow.Infrastructure/Services/WebSearchAgentService.cs
using AssetFlow.Application.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AssetFlow.Infrastructure.Services
{
    public class WebSearchAgentService : IWebSearchAgentService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration     _config;

        public WebSearchAgentService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config      = config;
        }

        public async Task<string> SearchAsync(string query)
        {
            var tavilyKey = _config["Tavily:ApiKey"];
            
            // ── 1. Recherche Tavily ──
            string searchContext = string.Empty;
            if (!string.IsNullOrWhiteSpace(tavilyKey))
            {
                try
                {
                    var http = _httpFactory.CreateClient();
                    var payload = new
                    {
                        api_key     = tavilyKey,
                        query       = query,
                        search_depth = "basic",
                        max_results = 5
                    };
                    var resp = await http.PostAsync(
                        "https://api.tavily.com/search",
                        new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        var doc  = JsonDocument.Parse(json);
                        var results = doc.RootElement.GetProperty("results");
                        var sb = new StringBuilder();
                        foreach (var r in results.EnumerateArray())
                        {
                            sb.AppendLine($"- {r.GetProperty("title").GetString()}: {r.GetProperty("content").GetString()}");
                        }
                        searchContext = sb.ToString();
                    }
                }
                catch { /* Tavily indisponible, on continue sans */ }
            }

            // ── 2. Groq LLM pour synthèse ──
            var groqKey = _config["GroqApiKey"];
            if (string.IsNullOrWhiteSpace(groqKey)) return searchContext.Length > 0 ? searchContext : "Recherche web non disponible.";

            var groqHttp = _httpFactory.CreateClient();
            groqHttp.DefaultRequestHeaders.Add("Authorization", $"Bearer {groqKey}");

            var systemPrompt = "Tu es un assistant spécialisé en gestion de stock et d'actifs IT. " +
                               "Réponds en français de manière concise et utile. " +
                               "Si des résultats de recherche sont fournis, base-toi dessus.";

            var userContent = searchContext.Length > 0
                ? $"Question: {query}\n\nRésultats de recherche:\n{searchContext}\n\nSynthétise une réponse utile."
                : query;

            var groqPayload = new
            {
                model       = "llama-3.3-70b-versatile",
                max_tokens  = 800,
                messages    = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userContent  }
                }
            };

            var groqResp = await groqHttp.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(groqPayload), Encoding.UTF8, "application/json"));

            if (!groqResp.IsSuccessStatusCode) return searchContext;

            var groqJson = await groqResp.Content.ReadAsStringAsync();
            var groqDoc  = JsonDocument.Parse(groqJson);
            return groqDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? searchContext;
        }
    }
}
