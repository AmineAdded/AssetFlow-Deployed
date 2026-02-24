// ============================================================
// AssetFlow.Infrastructure / Services / MaterielService.cs
// Implémentation du service matériel — accès EF Core direct
// ============================================================

using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Services
{
    /// <summary>
    /// Implémentation concrète de <see cref="IMaterielService"/>
    /// utilisant Entity Framework Core et SQL Server
    /// </summary>
    public class MaterielService : IMaterielService
    {
        private readonly AppDbContext _db;

        public MaterielService(AppDbContext db) => _db = db;

        // ── Helpers ──────────────────────────────────────────────

        /// <summary>Mappe une entité vers un DTO de lecture</summary>
        private static MaterielDto ToDto(Materiel m) => new()
        {
            Id            = m.Id,
            Reference     = m.Reference,
            Designation   = m.Designation,
            Description   = m.Description,
            Categorie     = m.Categorie,
            QuantiteStock = m.QuantiteStock,
            QuantiteMin   = m.QuantiteMin,
            Unite         = m.Unite,
            Emplacement   = m.Emplacement,
            Etat          = m.Etat.ToString(),
            ImageUrl      = m.ImageUrl,
            DateAjout     = m.DateAjout
        };

        /// <summary>Convertit une chaîne en enum EtatMateriel (tolérant)</summary>
        private static EtatMateriel ParseEtat(string etat) =>
            Enum.TryParse<EtatMateriel>(etat, true, out var e) ? e : EtatMateriel.Disponible;

        // ── Lecture ───────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<IEnumerable<MaterielDto>> GetAllAsync()
        {
            var list = await _db.Materiels
                .AsNoTracking()
                .OrderBy(m => m.Designation)
                .ToListAsync();

            return list.Select(ToDto);
        }

        /// <inheritdoc/>
        public async Task<MaterielDto?> GetByIdAsync(int id)
        {
            var m = await _db.Materiels.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return m is null ? null : ToDto(m);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MaterielDto>> SearchAsync(
            string? terme, string? categorie, string? etat)
        {
            var q = _db.Materiels.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(terme))
            {
                var t = terme.Trim().ToLower();
                q = q.Where(m =>
                    m.Designation.ToLower().Contains(t) ||
                    m.Reference.ToLower().Contains(t)   ||
                    (m.Description != null && m.Description.ToLower().Contains(t)));
            }

            if (!string.IsNullOrWhiteSpace(categorie) && categorie != "all")
                q = q.Where(m => m.Categorie.ToLower() == categorie.ToLower());

            if (!string.IsNullOrWhiteSpace(etat) && etat != "all")
            {
                var etatEnum = ParseEtat(etat);
                q = q.Where(m => m.Etat == etatEnum);
            }

            var list = await q.OrderBy(m => m.Designation).ToListAsync();
            return list.Select(ToDto);
        }

        /// <inheritdoc/>
        public async Task<MaterielStatsDto> GetStatsAsync()
        {
            var all = await _db.Materiels.AsNoTracking().ToListAsync();
            return new MaterielStatsDto
            {
                TotalArticles   = all.Count,
                EnStock         = all.Count(m => m.Etat == EtatMateriel.Disponible),
                // Alerte : stock entre QuantiteMin et QuantiteMin*2 (hors rupture)
                AlerteSeuil     = all.Count(m =>
                    m.Etat != EtatMateriel.EnRupture &&
                    m.QuantiteStock <= m.QuantiteMin * 2 &&
                    m.QuantiteStock > 0),
                RuptureCritique = all.Count(m => m.Etat == EtatMateriel.EnRupture || m.QuantiteStock == 0)
            };
        }

        // ── Écriture ──────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<MaterielResultDto> CreerAsync(CreerMaterielDto dto)
        {
            // Vérifie l'unicité de la référence
            if (await _db.Materiels.AnyAsync(m => m.Reference == dto.Reference.Trim()))
                return new MaterielResultDto { Succes = false, Message = "Cette référence existe déjà." };

            var materiel = new Materiel
            {
                Reference     = dto.Reference.Trim(),
                Designation   = dto.Designation.Trim(),
                Description   = dto.Description?.Trim(),
                Categorie     = dto.Categorie.Trim(),
                QuantiteStock = dto.QuantiteStock,
                QuantiteMin   = dto.QuantiteMin,
                Unite         = dto.Unite.Trim(),
                Emplacement   = dto.Emplacement?.Trim(),
                Etat          = ParseEtat(dto.Etat),
                ImageUrl      = dto.ImageUrl?.Trim(),
                DateAjout     = DateTime.UtcNow
            };

            _db.Materiels.Add(materiel);
            await _db.SaveChangesAsync();

            return new MaterielResultDto
            {
                Succes     = true,
                Message    = "Matériel créé avec succès.",
                IdMateriel = materiel.Id
            };
        }

        /// <inheritdoc/>
        public async Task<MaterielResultDto> ModifierAsync(ModifierMaterielDto dto)
        {
            var materiel = await _db.Materiels.FindAsync(dto.Id);
            if (materiel is null)
                return new MaterielResultDto { Succes = false, Message = "Matériel introuvable." };

            // Vérifie l'unicité de la référence (hors lui-même)
            if (await _db.Materiels.AnyAsync(m => m.Reference == dto.Reference.Trim() && m.Id != dto.Id))
                return new MaterielResultDto { Succes = false, Message = "Cette référence est déjà utilisée." };

            materiel.Reference     = dto.Reference.Trim();
            materiel.Designation   = dto.Designation.Trim();
            materiel.Description   = dto.Description?.Trim();
            materiel.Categorie     = dto.Categorie.Trim();
            materiel.QuantiteStock = dto.QuantiteStock;
            materiel.QuantiteMin   = dto.QuantiteMin;
            materiel.Unite         = dto.Unite.Trim();
            materiel.Emplacement   = dto.Emplacement?.Trim();
            materiel.Etat          = ParseEtat(dto.Etat);
            materiel.ImageUrl      = dto.ImageUrl?.Trim();

            await _db.SaveChangesAsync();
            return new MaterielResultDto { Succes = true, Message = "Matériel mis à jour." };
        }

        /// <inheritdoc/>
        public async Task<MaterielResultDto> SupprimerAsync(int id)
        {
            var materiel = await _db.Materiels.FindAsync(id);
            if (materiel is null)
                return new MaterielResultDto { Succes = false, Message = "Matériel introuvable." };

            // Sécurité : on ne supprime pas si des affectations existent
            var aAffectations = await _db.Affectations.AnyAsync(a => a.MaterielId == id);
            if (aAffectations)
                return new MaterielResultDto
                {
                    Succes  = false,
                    Message = "Ce matériel possède des affectations et ne peut pas être supprimé."
                };

            _db.Materiels.Remove(materiel);
            await _db.SaveChangesAsync();
            return new MaterielResultDto { Succes = true, Message = "Matériel supprimé." };
        }
    }
}   