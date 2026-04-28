// src/Backend/AssetFlow.Infrastructure/Services/WebSearchAgentService.cs
using AssetFlow.Application.DTOs.AgentDtos;
using AssetFlow.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AssetFlow.Infrastructure.Services
{
    public class WebSearchAgentService : IWebSearchAgentService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration     _config;

        public WebSearchAgentService(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config  = config;
        }

        public async Task<string> SearchAsync(string query, List<AgentChatHistory>? history = null)
        {
            var tavilyKey = _config["Tavily:ApiKey"];
            if (string.IsNullOrWhiteSpace(tavilyKey))
                return "❌ Clé API Tavily non configurée.";

            try
            {
                // ── 1. Résoudre la vraie requête de recherche via le contexte ──
                // Si la question est vague ("le meilleur ?", "compare-les"), on la résout
                // grâce à l'historique avant de lancer la recherche Tavily.
                var resolvedQuery = await ResolveQueryWithContextAsync(query, history);

                // ── 2. Tavily Search ──────────────────────────────────────────
                var http    = _factory.CreateClient();
                var payload = new
                {
                    api_key             = tavilyKey,
                    query               = resolvedQuery,
                    search_depth        = "basic",
                    include_answer      = true,
                    include_raw_content = false,
                    max_results         = 5,
                    include_domains     = Array.Empty<string>(),
                    exclude_domains     = Array.Empty<string>()
                };

                var tavilyResp = await http.PostAsJsonAsync(
                    "https://api.tavily.com/search", payload);

                if (!tavilyResp.IsSuccessStatusCode)
                    return $"❌ Erreur Tavily : {tavilyResp.StatusCode}";

                var tavilyJson = await tavilyResp.Content.ReadAsStringAsync();
                using var doc  = JsonDocument.Parse(tavilyJson);
                var root        = doc.RootElement;

                var answer = root.TryGetProperty("answer", out var ans)
                    ? ans.GetString() ?? ""
                    : "";

                var sources = new List<(string title, string url, string snippet)>();
                if (root.TryGetProperty("results", out var results))
                {
                    foreach (var r in results.EnumerateArray())
                    {
                        var title   = r.TryGetProperty("title",   out var t) ? t.GetString() ?? "" : "";
                        var url     = r.TryGetProperty("url",     out var u) ? u.GetString() ?? "" : "";
                        var snippet = r.TryGetProperty("content", out var c)
                            ? (c.GetString() ?? "")[..Math.Min(200, (c.GetString() ?? "").Length)]
                            : "";
                        if (!string.IsNullOrEmpty(url))
                            sources.Add((title, url, snippet));
                    }
                }

                // ── 3. Synthèse via LLM avec historique ──────────────────────
                var groqKey    = _config["GroqApiKey"];
                var mistralKey = _config["MistralApiKey"];

                var sourcesText = string.Join("\n", sources.Select((s, i) =>
                    $"[{i + 1}] {s.title}\nURL: {s.url}\nExtrait: {s.snippet}"));

                var systemPrompt = @"Tu es l'assistant IA d'AssetFlow, un système de gestion de stock.
Réponds en français de manière concise et utile.
Utilise le contexte de la conversation pour comprendre les références implicites (""le meilleur"", ""celui-là"", ""compare-les"", etc.).
Quand tu cites une information tirée d'une source, indique le numéro de la source entre crochets [1], [2], etc.
À la fin de ta réponse, liste TOUJOURS les sources utilisées sous forme de liens cliquables Markdown.
Format des sources : [Titre de la page](URL)";

                var historyContext = BuildHistoryForLlm(history);
                var userPrompt = $@"{historyContext}Question actuelle : {query}

Requête de recherche utilisée : {resolvedQuery}
Réponse directe disponible : {answer}

Sources trouvées :
{sourcesText}

Réponds en synthétisant ces informations. Cite les sources avec [1], [2], etc. 
Termine par une section '## Sources' avec les liens.";

                string synthesizedAnswer;

                if (!string.IsNullOrWhiteSpace(groqKey))
                    synthesizedAnswer = await CallGroqAsync(groqKey, systemPrompt, userPrompt);
                else if (!string.IsNullOrWhiteSpace(mistralKey))
                    synthesizedAnswer = await CallMistralAsync(mistralKey, systemPrompt, userPrompt);
                else
                {
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(answer)) sb.AppendLine(answer).AppendLine();
                    if (sources.Any())
                    {
                        sb.AppendLine("## Sources");
                        foreach (var (title, url, snippet) in sources)
                        {
                            sb.AppendLine($"- [{title}]({url})");
                            if (!string.IsNullOrEmpty(snippet))
                                sb.AppendLine($"  *{snippet.TrimEnd()}...*");
                        }
                    }
                    return sb.ToString();
                }

                if (!synthesizedAnswer.Contains("## Sources") && sources.Any())
                {
                    var sb = new StringBuilder(synthesizedAnswer);
                    sb.AppendLine("\n## Sources");
                    foreach (var (title, url, _) in sources)
                        sb.AppendLine($"- [{title}]({url})");
                    return sb.ToString();
                }

                return synthesizedAnswer;
            }
            catch (Exception ex)
            {
                return $"❌ Erreur lors de la recherche : {ex.Message}";
            }
        }

        // ── Résoudre une question vague grâce au contexte ────────────────────
        // Ex: "c'est quoi le meilleur ?" → "meilleur PC Asus gaming 2024"
        private async Task<string> ResolveQueryWithContextAsync(string query, List<AgentChatHistory>? history)
        {
            if (history == null || history.Count == 0) return query;

            // Si la question est déjà explicite (>= 5 mots significatifs), pas besoin de résolution
            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 5) return query;

            var groqKey = _config["GroqApiKey"];
            if (string.IsNullOrWhiteSpace(groqKey)) return query;

            try
            {
                var http = _factory.CreateClient();
                http.DefaultRequestHeaders.Add("Authorization", $"Bearer {groqKey}");

                var historyText = string.Join("\n", history.TakeLast(6).Select(h =>
                    $"  [{(h.Role == "user" ? "Utilisateur" : "Assistant")}]: {(h.Content.Length > 200 ? h.Content[..197] + "..." : h.Content)}"));

                var prompt = $@"Voici une conversation. Le dernier message de l'utilisateur est peut-être vague ou fait référence à quelque chose mentionné avant.
Génère UNE SEULE requête de recherche web optimisée, autonome et explicite, qui capture l'intention réelle.

Conversation :
{historyText}

Dernier message : ""{query}""

Réponds UNIQUEMENT avec la requête de recherche (pas d'explication, pas de guillemets) :";

                var payload = new
                {
                    model      = "llama-3.3-70b-versatile",
                    max_tokens = 50,
                    messages   = new[] { new { role = "user", content = prompt } }
                };

                var resp = await http.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (!resp.IsSuccessStatusCode) return query;

                var json = await resp.Content.ReadAsStringAsync();
                var doc  = JsonDocument.Parse(json);
                var resolved = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()?.Trim() ?? query;

                return string.IsNullOrWhiteSpace(resolved) ? query : resolved;
            }
            catch { return query; }
        }

        // ── Formater l'historique pour injection dans le prompt de synthèse ──
        private static string BuildHistoryForLlm(List<AgentChatHistory>? history)
        {
            if (history == null || history.Count <= 1) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("Contexte de la conversation précédente :");
            foreach (var h in history.SkipLast(1).TakeLast(6))
            {
                var role    = h.Role == "user" ? "Utilisateur" : "Assistant";
                var content = h.Content.Length > 300 ? h.Content[..297] + "..." : h.Content;
                sb.AppendLine($"  [{role}]: {content}");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        // ── Groq ─────────────────────────────────────────────────────────────
        private async Task<string> CallGroqAsync(string key, string system, string user)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);

            var body = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                    new { role = "system", content = system },
                    new { role = "user",   content = user   }
                },
                temperature = 0.3,
                max_tokens  = 1024
            };

            var resp = await http.PostAsJsonAsync("https://api.groq.com/openai/v1/chat/completions", body);
            if (!resp.IsSuccessStatusCode) return $"❌ Erreur Groq : {resp.StatusCode}";

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Pas de réponse.";
        }

        // ── Mistral ───────────────────────────────────────────────────────────
        private async Task<string> CallMistralAsync(string key, string system, string user)
        {
            var http = _factory.CreateClient("MistralClient");
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);

            var body = new
            {
                model    = "mistral-small-latest",
                messages = new[]
                {
                    new { role = "system", content = system },
                    new { role = "user",   content = user   }
                },
                temperature = 0.3,
                max_tokens  = 1024
            };

            var resp = await http.PostAsJsonAsync("/v1/chat/completions", body);
            if (!resp.IsSuccessStatusCode) return $"❌ Erreur Mistral : {resp.StatusCode}";

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Pas de réponse.";
        }
    }
}