// ============================================================
// AssetFlow.Infrastructure / Services / SentimentService.cs
//
// MODÈLE GRATUIT : HuggingFace Inference API
//   • Modèle principal : mistralai/Mistral-7B-Instruct-v0.3
//     (génération de texte, gratuit avec compte HF gratuit)
//   • Fallback automatique : analyse algorithmique locale
//     (sans aucune API si HuggingFace est indisponible)
//
// ⚠️  Clé HuggingFace gratuite : https://huggingface.co/settings/tokens
//     → créer un token "Read" (gratuit, sans carte bancaire)
//     → ajouter dans appsettings.json : "HuggingFace": { "ApiKey": "hf_..." }
// ============================================================

using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;


namespace AssetFlow.Infrastructure.Services
{
    public class SentimentService : ISentimentService
    {
        private readonly AppDbContext             _context;
        private readonly HttpClient               _http;
        private readonly string?                  _apiKey;
        private readonly ILogger<SentimentService> _logger;

        // ── HuggingFace Inference API — TOTALEMENT GRATUIT ───────
        // Mistral 7B : modèle de génération de texte haute qualité
        private const string HF_BASE_URL = "https://api-inference.huggingface.co/models/";
        private const string HF_MODEL    = "mistralai/Mistral-7B-Instruct-v0.3";

        public SentimentService(
            AppDbContext context,
            IHttpClientFactory httpFactory,
            IConfiguration config,
            ILogger<SentimentService> logger)
        {
            _context = context;
            _http    = httpFactory.CreateClient("HuggingFaceClient");
            _apiKey  = config["HuggingFace:ApiKey"]; // Optionnel — fonctionne sans clé (rate-limited)
            _logger  = logger;
        }

        // ── Analyse un seul matériel ──────────────────────────────
        public async Task<SentimentMaterielDto> AnalyserSentimentMaterielAsync(int materielId)
        {
            var materiel = await _context.Materiels
                .FirstOrDefaultAsync(m => m.Id == materielId)
                ?? throw new KeyNotFoundException($"Matériel {materielId} introuvable.");

            var commentaires = await _context.CommentairesMateriel
                .Where(c => c.MaterielId == materielId)
                .OrderByDescending(c => c.DateCreation)
                .Take(30) // limite raisonnable pour le contexte du modèle
                .Select(c => new SentimentCommentairePayload { Id = c.Id, Contenu = c.Contenu })
                .ToListAsync();

            if (!commentaires.Any())
            {
                return new SentimentMaterielDto
                {
                    MaterielId        = materielId,
                    MaterielRef       = materiel.Reference,
                    MaterielNom       = materiel.Designation,
                    TotalCommentaires = 0,
                    Resume            = "Aucun commentaire disponible.",
                    SentimentDominant = "Neutre",
                    ScoreGlobal       = 3
                };
            }

            // Essai avec HuggingFace, fallback algorithmique si échec
            try
            {
                var result = await AppelerHuggingFaceAsync(
                    materielId, materiel.Reference, materiel.Designation, commentaires);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "HuggingFace indisponible pour matériel {Id} — fallback algorithmique.", materielId);
                return AnalyseAlgorithmique(materielId, materiel.Reference, materiel.Designation, commentaires);
            }
        }

        // ── Analyse tous les matériels ────────────────────────────
        public async Task<List<SentimentMaterielDto>> AnalyserTousMaterielAsync()
        {
            var materielIds = await _context.CommentairesMateriel
                .Select(c => c.MaterielId)
                .Distinct()
                .ToListAsync();

            var resultats = new List<SentimentMaterielDto>();

            foreach (var id in materielIds)
            {
                try
                {
                    var r = await AnalyserSentimentMaterielAsync(id);
                    resultats.Add(r);
                    // Pause entre requêtes pour respecter le rate-limit HF gratuit
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur analyse sentiment matériel {Id}", id);
                }
            }

            return resultats.OrderByDescending(r => r.TotalCommentaires).ToList();
        }

        // ── Appel HuggingFace Inference API ──────────────────────
        private async Task<SentimentMaterielDto> AppelerHuggingFaceAsync(
            int materielId,
            string reference,
            string designation,
            List<SentimentCommentairePayload> commentaires)
        {
            var texteCommentaires = string.Join("\n", commentaires
                .Select((c, i) => $"- {c.Contenu}"));

            // Prompt structuré au format Mistral Instruct
            var prompt = $$$"""
                <s>[INST] Tu es un expert en analyse de sentiment pour des équipements informatiques en entreprise.

                Matériel : {designation} (Réf: {reference})
                Commentaires ({commentaires.Count}) :
                {texteCommentaires}

                Analyse ces commentaires. Réponds UNIQUEMENT avec ce JSON valide, sans markdown ni backticks :
                {{"positifs":{{"count":0,"pct":0.0}},"negatifs":{{"count":0,"pct":0.0}},"neutres":{{"count":0,"pct":0.0}},"score":3.0,"dominant":"Neutre","resume":"résumé court en français"}}

                Règles : positifs+negatifs+neutres={commentaires.Count}, score entre 1.0 et 5.0, dominant parmi Positif/Négatif/Neutre/Mitigé, resume max 120 caractères. [/INST]
                """;

            var requestBody = new
            {
                inputs = prompt,
                parameters = new
                {
                    max_new_tokens  = 200,
                    temperature     = 0.1,   // déterministe = meilleur JSON
                    return_full_text = false
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{HF_BASE_URL}{HF_MODEL}");

            // Clé optionnelle — sans clé : 30 req/h gratuits ; avec clé gratuite : meilleur quota
            if (!string.IsNullOrEmpty(_apiKey))
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _http.SendAsync(request);

            // HuggingFace renvoie 503 si le modèle est en train de charger (cold start)
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogInformation("Modèle HF en chargement, attente 15s…");
                await Task.Delay(15000);
                response = await _http.SendAsync(request);
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"HuggingFace API error {response.StatusCode}: {responseBody}");

            return ParseReponseHF(responseBody, materielId, reference, designation, commentaires.Count);
        }

        // ── Parser la réponse HuggingFace ─────────────────────────
        private SentimentMaterielDto ParseReponseHF(
            string responseBody,
            int materielId,
            string reference,
            string designation,
            int total)
        {
            try
            {
                // HF retourne un tableau : [{"generated_text": "..."}]
                using var doc = JsonDocument.Parse(responseBody);
                var generated = doc.RootElement[0]
                    .GetProperty("generated_text")
                    .GetString() ?? "";

                // Extraire le JSON de la réponse (entre { et })
                var start = generated.IndexOf('{');
                var end   = generated.LastIndexOf('}');
                if (start < 0 || end < 0)
                    throw new FormatException("Pas de JSON trouvé dans la réponse.");

                var jsonStr = generated[start..(end + 1)];

                using var sentDoc = JsonDocument.Parse(jsonStr);
                var root = sentDoc.RootElement;

                int positifs = root.GetProperty("positifs").GetProperty("count").GetInt32();
                int negatifs = root.GetProperty("negatifs").GetProperty("count").GetInt32();
                int neutres  = root.GetProperty("neutres").GetProperty("count").GetInt32();

                // Normaliser si somme incorrecte
                int somme = positifs + negatifs + neutres;
                if (somme != total && somme > 0)
                {
                    double f = (double)total / somme;
                    positifs = (int)Math.Round(positifs * f);
                    negatifs = (int)Math.Round(negatifs * f);
                    neutres  = total - positifs - negatifs;
                }
                if (neutres < 0) neutres = 0;

                double score    = root.GetProperty("score").GetDouble();
                string dominant = root.GetProperty("dominant").GetString() ?? "Neutre";
                string resume   = root.GetProperty("resume").GetString() ?? "";

                return new SentimentMaterielDto
                {
                    MaterielId          = materielId,
                    MaterielRef         = reference,
                    MaterielNom         = designation,
                    TotalCommentaires   = total,
                    Positifs            = positifs,
                    Negatifs            = negatifs,
                    Neutres             = neutres,
                    PourcentagePositif  = total > 0 ? Math.Round((double)positifs / total * 100, 1) : 0,
                    PourcentageNegatif  = total > 0 ? Math.Round((double)negatifs / total * 100, 1) : 0,
                    PourcentageNeutre   = total > 0 ? Math.Round((double)neutres  / total * 100, 1) : 0,
                    ScoreGlobal         = Math.Clamp(score, 1.0, 5.0),
                    SentimentDominant   = NormaliserDominant(dominant),
                    Resume              = resume.Length > 150 ? resume[..150] : resume
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Parsing HF échoué, fallback algorithmique.");
                throw; // remonté vers le caller qui appellera AnalyseAlgorithmique
            }
        }

        // ── Fallback : analyse algorithmique sans API ─────────────
        // Utilisé si HuggingFace est indisponible ou hors quota.
        // Basé sur un lexique français de mots positifs/négatifs.
        private static SentimentMaterielDto AnalyseAlgorithmique(
            int materielId,
            string reference,
            string designation,
            List<SentimentCommentairePayload> commentaires)
        {
            // Lexique simplifié francophone
            var motsPositifs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bien","bon","bonne","excellent","excellente","parfait","parfaite",
                "super","top","génial","rapide","fiable","efficace","pratique",
                "agréable","satisfait","satisfaite","content","contente","bravo",
                "qualité","solide","robuste","performant","facile","utile",
                "recommande","recommandé","apprécié","apprécie","j'adore",
                "aime","aimé","positif","merveilleux","formidable","nickel"
            };

            var motsNegatifs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "mauvais","mauvaise","nul","nulle","médiocre","décevant","décevante",
                "problème","problèmes","bug","bugs","lent","lente","cassé","cassée",
                "défaut","défauts","panne","pannes","inutile","fragile","bruyant",
                "cher","trop","pas","ne","n'est","difficile","compliqué",
                "insatisfait","insatisfaite","déçu","déçue","déception","horrible",
                "nettement","vraiment","franchement","malheureusement","regrette",
                "éviter","évitez","déconseille","retourné","retourner"
            };

            int positifs = 0, negatifs = 0, neutres = 0;

            foreach (var c in commentaires)
            {
                var mots = c.Contenu
                    .ToLower()
                    .Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '"', '\'', '-' },
                           StringSplitOptions.RemoveEmptyEntries);

                int scorePos = mots.Count(m => motsPositifs.Contains(m));
                int scoreNeg = mots.Count(m => motsNegatifs.Contains(m));

                if (scorePos > scoreNeg)      positifs++;
                else if (scoreNeg > scorePos) negatifs++;
                else                          neutres++;
            }

            int total = commentaires.Count;
            double pctPos = total > 0 ? Math.Round((double)positifs / total * 100, 1) : 0;
            double pctNeg = total > 0 ? Math.Round((double)negatifs / total * 100, 1) : 0;
            double pctNeu = total > 0 ? Math.Round((double)neutres  / total * 100, 1) : 0;

            double score = 3.0;
            if (total > 0) score = Math.Round(1.0 + (double)positifs / total * 4.0, 1);

            string dominant = "Neutre";
            if      (pctPos > 60) dominant = "Positif";
            else if (pctNeg > 60) dominant = "Négatif";
            else if (pctPos > 0 && pctNeg > 0) dominant = "Mitigé";

            string resume = dominant switch
            {
                "Positif" => $"Les utilisateurs sont globalement satisfaits de {designation}.",
                "Négatif" => $"Les utilisateurs expriment plusieurs insatisfactions sur {designation}.",
                "Mitigé"  => $"Les avis sont partagés sur {designation}.",
                _         => $"Les commentaires sur {designation} restent neutres ou factuels."
            };

            return new SentimentMaterielDto
            {
                MaterielId          = materielId,
                MaterielRef         = reference,
                MaterielNom         = designation,
                TotalCommentaires   = total,
                Positifs            = positifs,
                Negatifs            = negatifs,
                Neutres             = neutres,
                PourcentagePositif  = pctPos,
                PourcentageNegatif  = pctNeg,
                PourcentageNeutre   = pctNeu,
                ScoreGlobal         = score,
                SentimentDominant   = dominant,
                Resume              = resume + " (analyse locale)"
            };
        }

        // ── Normaliser le sentiment dominant ──────────────────────
        private static string NormaliserDominant(string raw) => raw.Trim() switch
        {
            var s when s.Contains("Positif",  StringComparison.OrdinalIgnoreCase) => "Positif",
            var s when s.Contains("Négatif",  StringComparison.OrdinalIgnoreCase) => "Négatif",
            var s when s.Contains("Negatif",  StringComparison.OrdinalIgnoreCase) => "Négatif",
            var s when s.Contains("Mitigé",   StringComparison.OrdinalIgnoreCase) => "Mitigé",
            var s when s.Contains("Mixte",    StringComparison.OrdinalIgnoreCase) => "Mitigé",
            var s when s.Contains("Mixed",    StringComparison.OrdinalIgnoreCase) => "Mitigé",
            _ => "Neutre"
        };
    }
}
