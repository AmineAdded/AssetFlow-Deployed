// ============================================================
// AssetFlow.Infrastructure / Services / AffectationService.cs
// Implémentation du service d'affectation de matériel
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Services
{
    public class AffectationService : IAffectationService
    {
        private readonly AppDbContext _db;

        public AffectationService(AppDbContext db) => _db = db;

        // ── Utilisateurs disponibles ──────────────────────────────
        public async Task<List<UtilisateurDisponibleDto>> GetUtilisateursDisponiblesAsync(string? search = null)
        {
            var query = _db.Users
                .AsNoTracking()
                .Where(u => u.Role == "Employe");

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(s) ||
                    u.LastName.ToLower().Contains(s)  ||
                    u.Email.ToLower().Contains(s)     ||
                    u.Department.ToLower().Contains(s));
            }

            var users = await query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToListAsync();

            return users.Select(u => new UtilisateurDisponibleDto
            {
                Id         = u.Id,
                FullName   = $"{u.FirstName} {u.LastName}",
                Email      = u.Email,
                Department = u.Department,
                Initials   = GetInitials(u.FirstName, u.LastName)
            }).ToList();
        }

        // ── Matériels disponibles ─────────────────────────────────
        public async Task<List<MaterielDisponibleDto>> GetMaterielsDisponiblesAsync(string? search = null)
        {
            var query = _db.Materiels
                .AsNoTracking()
                .Include(m => m.Affectations)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(m =>
                    m.Designation.ToLower().Contains(s) ||
                    m.Reference.ToLower().Contains(s)   ||
                    m.Categorie.ToLower().Contains(s));
            }

            var materiels = await query.OrderBy(m => m.Designation).ToListAsync();

            // Charger les articles disponibles par matériel
            var materielIds = materiels.Select(m => m.Id).ToList();
            var articles = await _db.ArticlesIndividuels
                .AsNoTracking()
                .Where(a => materielIds.Contains(a.MaterielId) && a.Statut == StatutArticle.Disponible)
                .ToListAsync();

            var result = new List<MaterielDisponibleDto>();

            foreach (var m in materiels)
            {
                var articlesMatériel = articles.Where(a => a.MaterielId == m.Id).ToList();
                if (articlesMatériel.Count == 0) continue; // Ignorer si aucun article dispo

                result.Add(new MaterielDisponibleDto
                {
                    Id                 = m.Id,
                    Reference          = m.Reference,
                    Designation        = m.Designation,
                    Categorie          = m.Categorie,
                    ImageUrl           = m.ImageUrl,
                    QuantiteDisponible = articlesMatériel.Count,
                    Articles           = articlesMatériel.Select(a => new ArticleDisponibleDto
                    {
                        Id          = a.Id,
                        NumeroSerie = a.NumeroSerie ?? $"S/N #{a.Id}",
                        Etat        = a.Etat.ToString()
                    }).ToList()
                });
            }

            return result;
        }

        // ── Créer affectation ─────────────────────────────────────
        public async Task<AffectationResultDto> CreerAffectationAsync(CreerAffectationDto dto)
        {
            // Validation
            if (dto.ArticleIds == null || dto.ArticleIds.Count == 0)
                return new AffectationResultDto { Succes = false, Message = "Aucun article sélectionné." };

            var utilisateur = await _db.Users.FindAsync(dto.UtilisateurId);
            if (utilisateur == null)
                return new AffectationResultDto { Succes = false, Message = "Utilisateur introuvable." };

            var materiel = await _db.Materiels.FindAsync(dto.MaterielId);
            if (materiel == null)
                return new AffectationResultDto { Succes = false, Message = "Matériel introuvable." };

            // Vérifier que tous les articles sont disponibles
            var articles = await _db.ArticlesIndividuels
                .Where(a => dto.ArticleIds.Contains(a.Id))
                .ToListAsync();

            if (articles.Count != dto.ArticleIds.Count)
                return new AffectationResultDto { Succes = false, Message = "Certains articles sont introuvables." };

            var articlesNonDisponibles = articles.Where(a => a.Statut != StatutArticle.Disponible).ToList();
            if (articlesNonDisponibles.Any())
                return new AffectationResultDto
                {
                    Succes  = false,
                    Message = $"{articlesNonDisponibles.Count} article(s) ne sont plus disponibles."
                };

            // Créer l'affectation
            var affectation = new Affectation
            {
                MaterielId       = dto.MaterielId,
                UtilisateurId    = dto.UtilisateurId,
                DateAffectation  = DateTime.UtcNow,
                QuantiteAffectee = articles.Count,
                Observations     = dto.Observations?.Trim(),
                DateRetour = dto.DateRetourPrevue
            };

            _db.Affectations.Add(affectation);
            await _db.SaveChangesAsync(); // Pour obtenir l'Id

            // Lier les articles à cette affectation
            foreach (var article in articles)
            {
                article.Statut        = StatutArticle.Affecte;
                article.AffectationId = affectation.Id;
            }

            // Décrémenter le stock
            materiel.QuantiteStock = Math.Max(0, materiel.QuantiteStock - articles.Count);

            await _db.SaveChangesAsync();

            return new AffectationResultDto
            {
                Succes        = true,
                Message       = $"Affectation créée avec succès pour {utilisateur.FirstName} {utilisateur.LastName}.",
                AffectationId = affectation.Id
            };
        }

        // ── Helper ────────────────────────────────────────────────
        private static string GetInitials(string firstName, string lastName)
        {
            var f = string.IsNullOrEmpty(firstName) ? "" : firstName[0].ToString().ToUpper();
            var l = string.IsNullOrEmpty(lastName)  ? "" : lastName[0].ToString().ToUpper();
            return f + l;
        }
    }
}