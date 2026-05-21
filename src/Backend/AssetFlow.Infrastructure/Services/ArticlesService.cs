using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Application.Services
{
    public class ArticleService : IArticleService
    {
        private readonly AppDbContext _db;
        public ArticleService(AppDbContext db) => _db = db;

        // ── Mise à jour numéro de série ───────────────────────────────────────
        public async Task<(bool Success, string Message, string? NumeroSerie)> UpdateNumeroSerieAsync(int id, string? numeroSerie)
        {
            var article = await _db.ArticlesIndividuels.FindAsync(id);
            if (article is null)
                return (false, "Article introuvable.", null);

            if (!string.IsNullOrWhiteSpace(numeroSerie))
            {
                var ns = numeroSerie.Trim();
                var existe = await _db.ArticlesIndividuels
                    .AnyAsync(a => a.NumeroSerie == ns && a.Id != id);
                if (existe)
                    return (false, "Ce numéro de série est déjà utilisé.", null);
                article.NumeroSerie = ns;
            }
            else
            {
                article.NumeroSerie = null;
            }

            await _db.SaveChangesAsync();
            return (true, "Numéro de série mis à jour.", article.NumeroSerie);
        }

        // ── Suppression définitive d'un article ───────────────────────────────
        public async Task<(bool Success, string Message)> SupprimerArticleAsync(int id)
        {
            var article = await _db.ArticlesIndividuels
                .Include(a => a.Materiel)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article is null)
                return (false, "Article introuvable.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                if (article.AffectationId.HasValue)
                {
                    var affectation = await _db.Affectations
                        .FirstOrDefaultAsync(a => a.Id == article.AffectationId.Value);

                    if (affectation != null)
                    {
                        affectation.QuantiteAffectee = Math.Max(0, affectation.QuantiteAffectee - 1);
                        var autresArticles = await _db.ArticlesIndividuels
                            .CountAsync(a => a.AffectationId == affectation.Id && a.Id != id);
                        if (autresArticles == 0)
                            affectation.Etat = EtatAffectation.Terminee;
                    }
                    article.AffectationId = null;
                }

                var incidents = await _db.Incidents
                    .Where(i => i.ArticleId.HasValue && i.ArticleId.Value == id)
                    .ToListAsync();
                if (incidents.Any()) _db.Incidents.RemoveRange(incidents);

                var historiques = await _db.ArticleHistoriques
                    .Where(h => h.ArticleId == id)
                    .ToListAsync();
                if (historiques.Any()) _db.ArticleHistoriques.RemoveRange(historiques);

                article.Materiel.QuantiteStock = Math.Max(0, article.Materiel.QuantiteStock - 1);
                _db.ArticlesIndividuels.Remove(article);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, $"Article #{id} supprimé définitivement.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Erreur lors de la suppression : {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // ── Mise hors service d'un article ────────────────────────────────────
        public async Task<(bool Success, string Message)> MettreHorsServiceAsync(int id)
        {
            var article = await _db.ArticlesIndividuels
                .Include(a => a.Materiel)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article is null)
                return (false, "Article introuvable.");

            if (article.Statut == StatutArticle.HorsService)
                return (false, "L'article est déjà hors service.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                bool etaitAffecte = article.AffectationId.HasValue;

                // 1. Retirer de l'affectation si affecté
                if (etaitAffecte)
                {
                    var affectation = await _db.Affectations
                        .FirstOrDefaultAsync(a => a.Id == article.AffectationId.Value);

                    if (affectation != null)
                    {
                        affectation.QuantiteAffectee = Math.Max(0, affectation.QuantiteAffectee - 1);

                        var autresArticles = await _db.ArticlesIndividuels
                            .CountAsync(a => a.AffectationId == affectation.Id && a.Id != id);
                        if (autresArticles == 0)
                            affectation.Etat = EtatAffectation.Terminee;
                    }

                    article.AffectationId = null;

                    // Si était affecté, le stock ne change pas (il était déjà sorti du stock)
                    // On passe juste à HorsService sans toucher au stock
                }
                // Si disponible : le stock diminue de 1 (l'article quitte le stock disponible)
                else if (article.Statut == StatutArticle.Disponible)
                {
                    article.Materiel.QuantiteStock = Math.Max(0, article.Materiel.QuantiteStock - 1);
                }

                // 2. Passer le statut à HorsService
                article.Statut = StatutArticle.HorsService;

                // 3. Enregistrer dans la biographie
                _db.ArticleHistoriques.Add(new ArticleHistorique
                {
                    ArticleId     = article.Id,
                    TypeEvenement = TypeEvenementArticle.MiseEnStock, // utilise Reforme si dispo, sinon PanneDeclaree
                    UtilisateurId = null,
                    DateEvenement = DateTime.UtcNow,
                    Description   = etaitAffecte
                        ? "Mis hors service — retiré de son affectation"
                        : "Mis hors service"
                });

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return (true, $"Article #{id} mis hors service.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Erreur : {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}