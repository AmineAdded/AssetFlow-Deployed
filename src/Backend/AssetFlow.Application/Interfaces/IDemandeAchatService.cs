// ============================================================
// COUCHE  : AssetFlow.Application
// FICHIER : Interfaces/IDemandeAchatService.cs
// RÔLE    : Contrat du service. Même pattern que IFournisseurService.cs
// ============================================================

using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Interfaces
{
    public interface IDemandeAchatService
    {
        // ── Lecture ─────────────────────────────────────────────

        /// <summary>Toutes les demandes avec leurs offres (sans binaire PDF)</summary>
        Task<List<DemandeAchat>> GetAllAsync();

        /// <summary>Une demande par son ID, avec ses offres</summary>
        Task<DemandeAchat?> GetByIdAsync(int id);

        // ── Statut ──────────────────────────────────────────────

        /// <summary>
        /// Change le statut d'une demande.
        /// Si statut = "refuse" → motifRefus obligatoire.
        /// Si statut = "traite" ou "refuse" → demande part en historique.
        /// </summary>
        Task ChangerStatutAsync(int idDemande, string statut, string? motifRefus = null);

        // ── Offres PDF ───────────────────────────────────────────

        /// <summary>Attache un PDF à une demande</summary>
        Task<OffreAchat> AjouterOffreAsync(int idDemande, OffreAchat offre);

        /// <summary>Supprime une offre par son GUID</summary>
        Task SupprimerOffreAsync(Guid idOffre);

        /// <summary>Retourne le binaire PDF pour l'aperçu / téléchargement</summary>
        Task<byte[]?> GetContenuPdfAsync(Guid idOffre);
    }
}
