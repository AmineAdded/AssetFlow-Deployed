// ============================================================
// AssetFlow.Domain / Entities / DemandeAchat.cs
// MODIF : ajout de LigneDemande (multi-matériel par demande)
//
// MIGRATION SQL à exécuter dans SSMS :
//
// CREATE TABLE LigneDemande (
//     IdLigne     INT IDENTITY(1,1) PRIMARY KEY,
//     IdDemande   INT          NOT NULL REFERENCES DemandeAchat(IdDemande) ON DELETE CASCADE,
//     NomProduit  VARCHAR(200) NOT NULL,
//     Quantite    INT          NOT NULL DEFAULT 1,
//     Description NVARCHAR(MAX) NULL
// );
// ============================================================

namespace AssetFlow.Domain.Entities
{
    /// <summary>
    /// Demande d'achat groupée — peut contenir plusieurs lignes de matériel.
    /// </summary>
    public class DemandeAchat
    {
        public int      IdDemande    { get; set; }
        public string   Reference    { get; set; } = string.Empty;

        /// <summary>
        /// Titre / résumé global de la demande (ex : "Équipements salle de réunion B").
        /// Remplace NomProduit unique — la liste des produits est dans Lignes.
        /// </summary>
        public string   NomProduit   { get; set; } = string.Empty;

        /// <summary>Conservé pour compatibilité — toujours 0, la vraie qté est par ligne.</summary>
        public int      Quantite     { get; set; } = 1;

        public string?  Description  { get; set; }
        public string   Statut       { get; set; } = "en_attente";
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public string   DemandeurNom { get; set; } = string.Empty;
        public string?  MotifRefus   { get; set; }

        // Navigation EF Core
        public List<OffreAchat>    Offres { get; set; } = new();
        public List<LigneDemande>  Lignes { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Ligne de matériel dans une demande d'achat.
    /// Une demande peut avoir 1..N lignes.
    /// </summary>
    public class LigneDemande
    {
        public int     IdLigne     { get; set; }
        public int     IdDemande   { get; set; }
        public string  NomProduit  { get; set; } = string.Empty;
        public int     Quantite    { get; set; } = 1;
        public string? Description { get; set; }

        // Navigation inverse
        public DemandeAchat? Demande { get; set; }
    }

    // ─────────────────────────────────────────────────────────

    /// <summary>Offre PDF attachée par l'Agent Achat à une demande.</summary>
    public class OffreAchat
    {
        public Guid    IdOffre     { get; set; } = Guid.NewGuid();
        public int     IdDemande   { get; set; }
        public string  NomFichier  { get; set; } = string.Empty;
        public long    Taille      { get; set; }
        public byte[]? ContenuPdf  { get; set; }
        public bool    EstChoisie  { get; set; } = false;
        public DemandeAchat? Demande { get; set; }
    }
}
