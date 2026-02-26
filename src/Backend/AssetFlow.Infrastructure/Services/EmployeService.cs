// ============================================================
// AssetFlow.Infrastructure / Services / EmployeService.cs
// MISE À JOUR : ajout GetMaterielsGroupesAsync
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Services
{
    public class EmployeService : IEmployeService
    {
        private readonly AppDbContext _context;

        public EmployeService(AppDbContext context)
        {
            _context = context;
        }

        // ── Existant : liste plate ─────────────────────────────
        public async Task<List<EquipementAffecteDto>> GetEquipementsAffectesAsync(int utilisateurId)
        {
            var affectations = await _context.Affectations
                .Include(a => a.Materiel)
                .Where(a => a.UtilisateurId == utilisateurId)
                .OrderByDescending(a => a.DateAffectation)
                .ToListAsync();

            return affectations.Select(a => new EquipementAffecteDto
            {
                AffectationId    = a.Id,
                MaterielId       = a.MaterielId,
                Reference        = a.Materiel.Reference,
                Designation      = a.Materiel.Designation,
                Categorie        = a.Materiel.Categorie,
                ImageUrl         = a.Materiel.ImageUrl,
                DateAffectation  = a.DateAffectation,
                QuantiteAffectee = a.QuantiteAffectee,
                Observations     = a.Observations
            }).ToList();
        }

        public async Task<EquipementAffecteDto?> GetEquipementDetailAsync(int affectationId)
        {
            var affectation = await _context.Affectations
                .Include(a => a.Materiel)
                .FirstOrDefaultAsync(a => a.Id == affectationId);

            if (affectation == null) return null;

            return new EquipementAffecteDto
            {
                AffectationId    = affectation.Id,
                MaterielId       = affectation.MaterielId,
                Reference        = affectation.Materiel.Reference,
                Designation      = affectation.Materiel.Designation,
                Categorie        = affectation.Materiel.Categorie,
                ImageUrl         = affectation.Materiel.ImageUrl,
                DateAffectation  = affectation.DateAffectation,
                QuantiteAffectee = affectation.QuantiteAffectee,
                Observations     = affectation.Observations
            };
        }

        // ── NOUVEAU : matériels groupés ────────────────────────
        /// <summary>
        /// Retourne les matériels distincts affectés à l'employé,
        /// chacun avec la liste de ses articles (affectations EnCours).
        ///
        /// Logique :
        ///   - On charge toutes les affectations EnCours de l'employé
        ///   - On les groupe par MaterielId
        ///   - Pour chaque groupe, on cherche l'ArticleIndividuel correspondant
        ///     via CommandeId + MaterielId (relation Article → Commande → Materiel)
        ///   - Si pas d'article individuel trouvé (matériel sans série),
        ///     on crée un article "virtuel" à partir de l'affectation
        /// </summary>
        public async Task<List<MaterielAffecteGroupeDto>> GetMaterielsGroupesAsync(int utilisateurId)
        {
            // 1. Charger toutes les affectations actives de l'employé
            var affectations = await _context.Affectations
                .Include(a => a.Materiel)
                .Where(a => a.UtilisateurId == utilisateurId)
                .OrderByDescending(a => a.DateAffectation)
                .ToListAsync();

            if (!affectations.Any())
                return new List<MaterielAffecteGroupeDto>();

            // 2. Récupérer tous les articles individuels liés aux matériels concernés
            var materielIds = affectations.Select(a => a.MaterielId).Distinct().ToList();
            var articles = await _context.ArticlesIndividuels
                .Where(art => materielIds.Contains(art.MaterielId)
                           && art.Statut == StatutArticle.Affecte)
                .ToListAsync();

            // 3. Grouper par matériel
            var groupes = affectations
                .GroupBy(a => a.MaterielId)
                .Select(g =>
                {
                    var premiereMat = g.First().Materiel;

                    // Articles individuels pour ce matériel
                    var articlesMatériel = articles
                        .Where(art => art.MaterielId == g.Key)
                        .ToList();

                    // Construire la liste des ArticleAffecteDto
                    // Stratégie : on associe chaque affectation à un article si possible
                    var articlesDto = new List<ArticleAffecteDto>();

                    foreach (var affectation in g)
                    {
                        // Tenter de trouver un article individuel non encore associé
                        var article = articlesMatériel.FirstOrDefault(
                            art => !articlesDto.Any(dto => dto.ArticleId == art.Id));

                        articlesDto.Add(new ArticleAffecteDto
                        {
                            AffectationId    = affectation.Id,
                            ArticleId        = article?.Id ?? 0,
                            NumeroSerie      = article?.NumeroSerie
                                               ?? $"S/N non renseigné — Aff. #{affectation.Id}",
                            StatutArticle    = article?.Statut.ToString() ?? "Affecte",
                            DateAffectation  = affectation.DateAffectation,
                            Observations     = affectation.Observations
                        });
                    }

                    return new MaterielAffecteGroupeDto
                    {
                        MaterielId          = g.Key,
                        Reference           = premiereMat.Reference,
                        Designation         = premiereMat.Designation,
                        Categorie           = premiereMat.Categorie,
                        ImageUrl            = premiereMat.ImageUrl,
                        NombreArticles      = g.Count(),
                        DerniereAffectation = g.Max(a => a.DateAffectation),
                        Articles            = articlesDto
                    };
                })
                .OrderByDescending(m => m.DerniereAffectation)
                .ToList();

            return groupes;
        }

        // ── Helpers ────────────────────────────────────────────

    }
}