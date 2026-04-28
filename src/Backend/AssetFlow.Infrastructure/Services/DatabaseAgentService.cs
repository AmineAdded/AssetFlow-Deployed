// src/Backend/AssetFlow.Infrastructure/Services/DatabaseAgentService.cs
using AssetFlow.Application.DTOs.AgentDtos;
using AssetFlow.Application.Interfaces;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AssetFlow.Infrastructure.Services
{
    public class DatabaseAgentService : IDatabaseAgentService
    {
        private readonly AppDbContext       _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration    _config;

        public DatabaseAgentService(AppDbContext db, IHttpClientFactory httpFactory, IConfiguration config)
        {
            _db          = db;
            _httpFactory = httpFactory;
            _config      = config;
        }

        // ── Alertes stock ──────────────────────────────────────────────────
        public async Task<List<AlerteStock>> GetStockAlertsAsync()
        {
            var materiels = await _db.Materiels
                .Where(m => m.QuantiteStock <= m.QuantiteMin)
                .AsNoTracking()
                .ToListAsync();

            return materiels.Select(m => new AlerteStock
            {
                MaterielId    = m.Id,
                Reference     = m.Reference,
                Designation   = m.Designation,
                QuantiteStock = m.QuantiteStock,
                QuantiteMin   = m.QuantiteMin,
                Categorie     = m.Categorie
            }).ToList();
        }

        // ── Réponse LLM basée sur les données DB (avec historique) ────────
        public async Task<string> QueryAsync(string question, List<AgentChatHistory>? history = null)
        {
            var context = await BuildDbContextAsync();
            var groqKey = _config["GroqApiKey"];
            if (string.IsNullOrWhiteSpace(groqKey)) return "Service IA non disponible.";

            var http = _httpFactory.CreateClient();
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {groqKey}");

            var systemPrompt = $@"Tu es un assistant de gestion d'actifs (AssetFlow). 
Tu as accès aux données suivantes de la base de données :

{context}

Réponds en français, de manière précise, aux questions sur les matériels, commandes, affectations, incidents.
Si on te demande d'ajouter/créer quelque chose, indique que tu vas préparer une proposition.
Ne fabrique pas de données qui n'existent pas dans le contexte fourni.
Utilise le contexte de la conversation pour comprendre les références comme ""celui-là"", ""le même"", ""combien il en reste"", etc.";

            // Construire les messages avec l'historique complet
            var messages = BuildMessagesWithHistory(history, systemPrompt, question);

            var payload = new
            {
                model      = "llama-3.3-70b-versatile",
                max_tokens = 1000,
                messages
            };

            var resp = await http.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode) return "Erreur lors de la consultation de la base de données.";

            var json = await resp.Content.ReadAsStringAsync();
            var doc  = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Réponse vide.";
        }

        // ── Contexte DB pour le LLM ──────────────────────────────────────
        private async Task<string> BuildDbContextAsync()
        {
            var sb = new StringBuilder();

            var materiels = await _db.Materiels.AsNoTracking()
                .OrderBy(m => m.Designation).Take(50).ToListAsync();
            sb.AppendLine("=== MATÉRIELS ===");
            foreach (var m in materiels)
                sb.AppendLine($"- [{m.Id}] {m.Reference} | {m.Designation} | Catégorie: {m.Categorie} | Stock: {m.QuantiteStock} {m.Unite} (min: {m.QuantiteMin}) | Emplacement: {m.Emplacement ?? "N/A"}");

            var commandes = await _db.Commandes
                .Include(c => c.Materiel)
                .Include(c => c.Fournisseur)
                .Include(c => c.Articles)
                .AsNoTracking()
                .OrderByDescending(c => c.DateAchat)
                .Take(20)
                .ToListAsync();
            sb.AppendLine("\n=== COMMANDES RÉCENTES ===");
            foreach (var c in commandes)
                sb.AppendLine($"- [{c.Id}] {c.NumeroCommande} | Matériel: {c.Materiel?.Designation} | Fournisseur: {c.Fournisseur?.Nom ?? "N/A"} | Qté: {c.QuantiteAchetee} | Date: {c.DateAchat:dd/MM/yyyy} | Articles: {c.Articles.Count}");

            var fournisseurs = await _db.Fournisseurs.AsNoTracking().Take(30).ToListAsync();
            sb.AppendLine("\n=== FOURNISSEURS ===");
            foreach (var f in fournisseurs)
                sb.AppendLine($"- [{f.IdFournisseur}] {f.Nom} | Tel: {f.Telephone ?? "N/A"} | Email: {f.Mail ?? "N/A"}");

            var totalMat  = await _db.Materiels.CountAsync();
            var totalCmd  = await _db.Commandes.CountAsync();
            var totalArt  = await _db.ArticlesIndividuels.CountAsync();
            var alertes   = await _db.Materiels.CountAsync(m => m.QuantiteStock <= m.QuantiteMin);
            var incidents = await _db.Incidents.CountAsync(i => i.Statut == AssetFlow.Domain.Entities.StatutIncident.EnAttente);

            sb.AppendLine("\n=== STATISTIQUES ===");
            sb.AppendLine($"Total matériels: {totalMat} | Total commandes: {totalCmd} | Total articles: {totalArt}");
            sb.AppendLine($"Matériels en alerte stock: {alertes} | Incidents en attente: {incidents}");

            return sb.ToString();
        }

        // ── Construit le tableau messages[] avec historique ───────────────
        private static object[] BuildMessagesWithHistory(
            List<AgentChatHistory>? history,
            string systemPrompt,
            string currentQuestion)
        {
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            if (history != null)
            {
                // On prend les N-1 derniers (le dernier = question actuelle, déjà ajoutée après)
                foreach (var h in history.SkipLast(1).TakeLast(8))
                    messages.Add(new { role = h.Role, content = h.Content });
            }

            messages.Add(new { role = "user", content = currentQuestion });
            return messages.ToArray();
        }
    }
}