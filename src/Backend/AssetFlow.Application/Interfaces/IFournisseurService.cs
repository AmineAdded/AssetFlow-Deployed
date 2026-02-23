// ============================================================
// COUCHE  : AssetFlow.Application
// FICHIER : Interfaces/IFournisseurService.cs
// RÔLE    : Contrat abstrait du service fournisseur.
//           Placé ici comme IAuthService, IEmployeService, IIncidentService.
//           L'implémentation concrète est dans Infrastructure/Services.
// ============================================================

using AssetFlow.Domain.Entities;

namespace AssetFlow.Application.Interfaces
{
    /// <summary>
    /// Contrat du service fournisseur.
    /// Définit toutes les opérations CRUD disponibles.
    /// Implémenté dans AssetFlow.Infrastructure/Services/FournisseurService.cs
    /// </summary>
    public interface IFournisseurService
    {
        // ── Lecture ──────────────────────────────────────────────

        /// <summary>Retourne la liste complète des fournisseurs</summary>
        Task<List<Fournisseur>> GetAllAsync();

        /// <summary>
        /// Retourne un fournisseur par son ID.
        /// Retourne null si introuvable.
        /// </summary>
        Task<Fournisseur?> GetByIdAsync(int id);

        /// <summary>
        /// Recherche des fournisseurs par Nom, Téléphone, Adresse ou Mail.
        /// Recherche insensible à la casse.
        /// </summary>
        Task<List<Fournisseur>> RechercherAsync(string terme);

        // ── Écriture ─────────────────────────────────────────────

        /// <summary>
        /// Crée un nouveau fournisseur.
        /// Retourne l'entité avec l'IdFournisseur généré.
        /// </summary>
        Task<Fournisseur> AjouterAsync(Fournisseur fournisseur);

        /// <summary>Met à jour un fournisseur existant</summary>
        Task ModifierAsync(Fournisseur fournisseur);

        /// <summary>Supprime un fournisseur par son ID</summary>
        Task SupprimerAsync(int id);
    }
}
