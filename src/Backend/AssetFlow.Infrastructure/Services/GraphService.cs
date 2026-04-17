using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Services
{
    /// <summary>
    /// Construit le graphe de la mémoire intelligente à partir des données réelles.
    /// Chaque matériel, utilisateur actif et incident devient un nœud.
    /// Les affectations et incidents créent les liens.
    /// Les insights sont générés par des règles simples (taux d'incident, pannes répétées, etc.)
    /// </summary>
    public class GraphService : IGraphService
    {
        private readonly AppDbContext _db;

        public GraphService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<GraphResponseDto> GetGraphAsync()
        {
            // ─── Chargement des données ───────────────────────────────────────
            var materiels = await _db.Materiels
                .AsNoTracking()
                .Take(30) // Limite pour la lisibilité du graphe
                .ToListAsync();

            var affectations = await _db.Affectations
                .AsNoTracking()
                .Include(a => a.Utilisateur)
                .Include(a => a.Materiel)
                .Where(a => a.Etat == EtatAffectation.Courante)
                .Take(30)
                .ToListAsync();

            var incidents = await _db.Incidents
                .AsNoTracking()
                .Include(i => i.Affectation).ThenInclude(a => a.Materiel)
                .Where(i => i.Statut != StatutIncident.Cloture)
                .Take(20)
                .ToListAsync();

            var users = affectations
                .Where(a => a.Utilisateur != null)
                .Select(a => a.Utilisateur!)
                .DistinctBy(u => u.Id)
                .ToList();

            // ─── Nœuds ────────────────────────────────────────────────────────
            var nodes = new List<GraphNodeDto>();
            var links = new List<GraphLinkDto>();

            // Matériels
            foreach (var m in materiels)
            {
                var incidentCount = incidents.Count(i => i.Affectation?.MaterielId == m.Id);
                nodes.Add(new GraphNodeDto
                {
                    Id     = $"m-{m.Id}",
                    Type   = "materiel",
                    Label  = m.Reference,
                    Detail = $"{m.Designation} | Stock: {m.QuantiteStock}",
                    Status = incidentCount > 2 ? "critical" : incidentCount > 0 ? "warning" : "normal",
                    Weight = Math.Min(1 + incidentCount, 5)
                });
            }

            // Utilisateurs (dédupliqués depuis les affectations courantes)
            foreach (var u in users)
            {
                nodes.Add(new GraphNodeDto
                {
                    Id     = $"u-{u.Id}",
                    Type   = "utilisateur",
                    Label  = $"{u.FirstName} {u.LastName[..1]}.",
                    Detail = $"{u.Department} | {u.Role}",
                    Status = "normal",
                    Weight = 2
                });
            }

            // Incidents (ouverts / en cours)
            foreach (var i in incidents)
            {
                nodes.Add(new GraphNodeDto
                {
                    Id     = $"i-{i.Id}",
                    Type   = "incident",
                    Label  = i.TypeIncident,
                    Detail = i.Description.Length > 60 ? i.Description[..60] + "…" : i.Description,
                    Status = i.Urgence >= 3 ? "critical" : "warning",
                    Weight = i.Urgence
                });
            }

            // Nœud IA central
            nodes.Add(new GraphNodeDto
            {
                Id     = "ia-core",
                Type   = "ia",
                Label  = "AssetFlow AI",
                Detail = "Moteur d'analyse intelligente",
                Status = "normal",
                Weight = 5
            });

            // ─── Liens ────────────────────────────────────────────────────────

            // Matériel ↔ Utilisateur (via affectation courante)
            foreach (var aff in affectations)
            {
                var mNode = nodes.FirstOrDefault(n => n.Id == $"m-{aff.MaterielId}");
                var uNode = aff.UtilisateurId.HasValue
                    ? nodes.FirstOrDefault(n => n.Id == $"u-{aff.UtilisateurId}")
                    : null;

                if (mNode != null && uNode != null)
                {
                    links.Add(new GraphLinkDto
                    {
                        Source   = mNode.Id,
                        Target   = uNode.Id,
                        Label    = "affecté à",
                        Strength = 0.6
                    });
                }
            }

            // Incident ↔ Matériel
            foreach (var inc in incidents)
            {
                var mId = $"m-{inc.Affectation?.MaterielId}";
                var iId = $"i-{inc.Id}";
                if (nodes.Any(n => n.Id == mId))
                {
                    links.Add(new GraphLinkDto
                    {
                        Source   = iId,
                        Target   = mId,
                        Label    = "signalé sur",
                        Strength = 0.8
                    });
                }
            }

            // IA ↔ matériels critiques
            foreach (var n in nodes.Where(n => n.Status == "critical"))
            {
                links.Add(new GraphLinkDto
                {
                    Source   = "ia-core",
                    Target   = n.Id,
                    Label    = "analyse",
                    Strength = 0.3
                });
            }

            // ─── Insights ─────────────────────────────────────────────────────
            var insights = new List<GraphInsightDto>();

            // Matériels avec taux d'incidents élevé
            foreach (var m in materiels)
            {
                var count = incidents.Count(i => i.Affectation?.MaterielId == m.Id);
                if (count >= 2)
                {
                    insights.Add(new GraphInsightDto
                    {
                        Type      = "warning",
                        Title     = "Anomalie détectée",
                        Message   = $"{m.Reference} présente {count} incident(s) actif(s). Vérification recommandée.",
                        EntityId  = $"m-{m.Id}",
                        GeneratedAt = DateTime.UtcNow
                    });
                }
            }

            // Stock critique
            foreach (var m in materiels.Where(m => m.QuantiteStock <= m.QuantiteMin))
            {
                insights.Add(new GraphInsightDto
                {
                    Type      = "recommendation",
                    Title     = "Stock critique",
                    Message   = $"{m.Designation} : stock ({m.QuantiteStock}) en dessous du seuil minimum ({m.QuantiteMin}).",
                    EntityId  = $"m-{m.Id}",
                    GeneratedAt = DateTime.UtcNow
                });
            }

            // Corrélation : utilisateurs avec plusieurs incidents
            var userIncidentGroups = incidents
                .Where(i => i.Affectation?.UtilisateurId.HasValue == true)
                .GroupBy(i => i.Affectation!.UtilisateurId!.Value)
                .Where(g => g.Count() >= 2);

            foreach (var group in userIncidentGroups)
            {
                var user = users.FirstOrDefault(u => u.Id == group.Key);
                if (user != null)
                {
                    insights.Add(new GraphInsightDto
                    {
                        Type      = "correlation",
                        Title     = "Corrélation trouvée",
                        Message   = $"{user.FirstName} {user.LastName} est lié(e) à {group.Count()} incidents. Analyse comportementale suggérée.",
                        EntityId  = $"u-{user.Id}",
                        GeneratedAt = DateTime.UtcNow
                    });
                }
            }

            // Insight IA global
            insights.Add(new GraphInsightDto
            {
                Type      = "info",
                Title     = "Analyse du parc",
                Message   = $"Graphe actif : {nodes.Count} entités, {links.Count} relations analysées. Taux de santé global : {ComputeHealthScore(materiels.Count, incidents.Count)}%",
                GeneratedAt = DateTime.UtcNow
            });

            // ─── Stats ────────────────────────────────────────────────────────
            var stats = new GraphStatsDto
            {
                TotalMateriel   = await _db.Materiels.CountAsync(),
                TotalIncidents  = await _db.Incidents.CountAsync(i => i.Statut != StatutIncident.Cloture),
                TotalUsers      = await _db.Users.CountAsync(),
                ActiveAnomalies = incidents.Count(i => i.Urgence >= 3)
            };

            return new GraphResponseDto
            {
                Nodes    = nodes,
                Links    = links,
                Insights = insights.OrderByDescending(i => i.GeneratedAt).ToList(),
                Stats    = stats
            };
        }

        public async Task<GraphInsightDto?> GetInsightForNodeAsync(string nodeId)
        {
            // Insight spécifique au clic sur un nœud
            if (nodeId.StartsWith("m-") && int.TryParse(nodeId[2..], out var mId))
            {
                var m = await _db.Materiels.FindAsync(mId);
                if (m == null) return null;

                var incCount = await _db.Incidents
                    .CountAsync(i => i.Affectation!.MaterielId == mId && i.Statut != StatutIncident.Cloture);

                return new GraphInsightDto
                {
                    Type      = incCount > 0 ? "warning" : "info",
                    Title     = m.Reference,
                    Message   = $"{m.Designation}\nCatégorie : {m.Categorie}\nStock : {m.QuantiteStock} {m.Unite}\nIncidents actifs : {incCount}",
                    EntityId  = nodeId,
                    GeneratedAt = DateTime.UtcNow
                };
            }

            if (nodeId.StartsWith("i-") && int.TryParse(nodeId[2..], out var iId))
            {
                var inc = await _db.Incidents
                    .Include(i => i.Affectation).ThenInclude(a => a.Materiel)
                    .FirstOrDefaultAsync(i => i.Id == iId);
                if (inc == null) return null;

                return new GraphInsightDto
                {
                    Type      = inc.Urgence >= 3 ? "warning" : "info",
                    Title     = inc.TypeIncident,
                    Message   = $"{inc.Description}\nUrgence : {inc.Urgence}/5\nStatut : {inc.Statut}\nMatériel : {inc.Affectation?.Materiel?.Reference ?? "—"}",
                    EntityId  = nodeId,
                    GeneratedAt = DateTime.UtcNow
                };
            }

            return null;
        }

        private static int ComputeHealthScore(int totalMateriel, int activeIncidents)
        {
            if (totalMateriel == 0) return 100;
            var ratio = (double)activeIncidents / totalMateriel;
            return Math.Max(0, (int)((1 - ratio) * 100));
        }
    }
}