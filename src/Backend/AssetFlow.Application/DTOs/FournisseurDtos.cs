// ============================================================
// COUCHE  : AssetFlow.Application
// FICHIER : DTOs/FournisseurDtos.cs
// RÔLE    : Objets de transfert entre le Controller et le Frontend.
//           Même pattern que AuthDtos.cs, EmployeDtos.cs, IncidentDtos.cs
// ============================================================

namespace AssetFlow.Application.DTOs
{
    // ──────────────────────────────────────────────────────────
    // DTO LECTURE — retourné au frontend lors d'un GET
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// DTO complet retourné par l'API lors d'une consultation.
    /// Utilisé pour afficher un fournisseur dans la liste ou le formulaire.
    /// </summary>
    public class FournisseurDto
    {
        public int     IdFournisseur { get; set; }
        public string  Nom          { get; set; } = string.Empty;
        public string? Telephone    { get; set; }
        public string? Adresse      { get; set; }
        public string? Mail         { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // DTO CRÉATION — reçu par l'API lors d'un POST
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// DTO utilisé lors de l'ajout d'un nouveau fournisseur.
    /// Pas d'IdFournisseur (généré automatiquement côté SQL Server).
    /// </summary>
    public class CreerFournisseurDto
    {
        public string  Nom       { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Adresse   { get; set; }
        public string? Mail      { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // DTO MODIFICATION — reçu par l'API lors d'un PUT
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// DTO utilisé lors de la modification d'un fournisseur existant.
    /// Contient l'IdFournisseur pour identifier l'enregistrement à modifier.
    /// </summary>
    public class ModifierFournisseurDto
    {
        public int     IdFournisseur { get; set; }
        public string  Nom          { get; set; } = string.Empty;
        public string? Telephone    { get; set; }
        public string? Adresse      { get; set; }
        public string? Mail         { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // DTO RÉPONSE — retourné après POST / PUT / DELETE
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Réponse standard pour les opérations d'écriture.
    /// Indique si l'opération a réussi et retourne un message.
    /// </summary>
    public class FournisseurReponseDto
    {
        /// <summary>True = succès, False = erreur</summary>
        public bool   Succes  { get; set; }

        /// <summary>Message lisible à afficher dans le toast/notification</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>ID du fournisseur créé (utile après un POST)</summary>
        public int?   IdFournisseur { get; set; }
    }
}
