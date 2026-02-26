// ============================================================
// COUCHE  : AssetFlow.Infrastructure
// FICHIER : Services/DemandeAchatService.cs
// RÔLE    : Implémentation concrète de IDemandeAchatService.
//           Même pattern que FournisseurService.cs
//
// ENREGISTREMENT dans Program.cs (WebApi) :
//   builder.Services.AddScoped<IDemandeAchatService, DemandeAchatService>();
// ============================================================

using AssetFlow.Application.Interfaces;
using AssetFlow.Domain.Entities;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Services
{
    public class DemandeAchatService : IDemandeAchatService
    {
        private readonly AppDbContext _context;

        public DemandeAchatService(AppDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────
        // GET ALL
        // Retourne toutes les demandes + offres (sans binaire PDF)
        // ────────────────────────────────────────────────────────

        public async Task<List<DemandeAchat>> GetAllAsync()
        {
            return await _context.DemandeAchat
                .Include(d => d.Offres)
                .OrderByDescending(d => d.DateCreation)
                .AsNoTracking()
                .ToListAsync();
        }

        // ────────────────────────────────────────────────────────
        // GET BY ID
        // ────────────────────────────────────────────────────────

        public async Task<DemandeAchat?> GetByIdAsync(int id)
        {
            return await _context.DemandeAchat
                .Include(d => d.Offres)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDemande == id);
        }

        // ────────────────────────────────────────────────────────
        // CHANGER STATUT
        // en_attente → commande → traite (archivé)
        //                       → refuse (archivé, motif obligatoire)
        // ────────────────────────────────────────────────────────

        public async Task ChangerStatutAsync(
            int idDemande, string statut, string? motifRefus = null)
        {
            var demande = await _context.DemandeAchat
                .FirstOrDefaultAsync(d => d.IdDemande == idDemande);

            if (demande == null)
                throw new KeyNotFoundException(
                    $"Demande ID {idDemande} introuvable.");

            var statutsValides = new[] { "en_attente", "commande", "traite", "refuse" };
            if (!statutsValides.Contains(statut))
                throw new ArgumentException($"Statut invalide : {statut}");

            if (statut == "refuse" && string.IsNullOrWhiteSpace(motifRefus))
                throw new ArgumentException(
                    "Le motif est obligatoire pour refuser une demande.");

            demande.Statut = statut;

            if (statut == "refuse")
                demande.MotifRefus = motifRefus!.Trim();

            await _context.SaveChangesAsync();
        }

        // ────────────────────────────────────────────────────────
        // AJOUTER UNE OFFRE PDF
        // ────────────────────────────────────────────────────────

        public async Task<OffreAchat> AjouterOffreAsync(int idDemande, OffreAchat offre)
        {
            var existe = await _context.DemandeAchat
                .AnyAsync(d => d.IdDemande == idDemande);

            if (!existe)
                throw new KeyNotFoundException(
                    $"Demande ID {idDemande} introuvable.");

            offre.IdDemande  = idDemande;
            offre.EstChoisie = false;

            _context.OffreAchat.Add(offre);
            await _context.SaveChangesAsync();

            return offre;
        }

        // ────────────────────────────────────────────────────────
        // SUPPRIMER UNE OFFRE
        // ────────────────────────────────────────────────────────

        public async Task SupprimerOffreAsync(Guid idOffre)
        {
            var offre = await _context.OffreAchat
                .FirstOrDefaultAsync(o => o.IdOffre == idOffre);

            if (offre == null)
                throw new KeyNotFoundException($"Offre {idOffre} introuvable.");

            _context.OffreAchat.Remove(offre);
            await _context.SaveChangesAsync();
        }

        // ────────────────────────────────────────────────────────
        // GET CONTENU PDF
        // Retourne les octets du PDF pour l'affichage dans l'iframe
        // ────────────────────────────────────────────────────────

        public async Task<byte[]?> GetContenuPdfAsync(Guid idOffre)
        {
            var offre = await _context.OffreAchat
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.IdOffre == idOffre);

            return offre?.ContenuPdf;
        }
    }
}
