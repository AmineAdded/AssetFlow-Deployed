// ============================================================
// AssetFlow.Application / DTOs / DemandeAchatDtos.cs
// MODIF : ajout LigneDemandeDto + CreateLigneDemandeDto
// ============================================================

namespace AssetFlow.Application.DTOs
{
    // ── LECTURE ─────────────────────────────────────────────────

    /// <summary>DTO d'une ligne de matériel dans une demande.</summary>
    public class LigneDemandeDto
    {
        public int     IdLigne     { get; set; }
        public string  NomProduit  { get; set; } = string.Empty;
        public int     Quantite    { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO complet d'une demande retourné par l'API.
    /// Inclut les lignes de matériel et les offres (sans binaire PDF).
    /// </summary>
    public class DemandeAchatDto
    {
        public int      IdDemande    { get; set; }
        public string   Reference    { get; set; } = string.Empty;

        /// <summary>Titre / résumé global de la demande.</summary>
        public string   NomProduit   { get; set; } = string.Empty;

        /// <summary>Quantité totale calculée (somme des lignes) — pour compatibilité.</summary>
        public int      Quantite     { get; set; }

        public string?  Description  { get; set; }
        public string   Statut       { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
        public string   DemandeurNom { get; set; } = string.Empty;
        public string?  MotifRefus   { get; set; }

        public List<LigneDemandeDto> Lignes { get; set; } = new();
        public List<OffreAchatDto>   Offres { get; set; } = new();
    }

    /// <summary>DTO d'une offre PDF — sans le binaire.</summary>
    public class OffreAchatDto
    {
        public Guid   IdOffre    { get; set; }
        public int    IdDemande  { get; set; }
        public string NomFichier { get; set; } = string.Empty;
        public long   Taille     { get; set; }
        public bool   EstChoisie { get; set; }
    }

    // ── CRÉATION ────────────────────────────────────────────────

    /// <summary>Ligne de matériel dans le formulaire de création.</summary>
    public class CreateLigneDemandeDto
    {
        public string  NomProduit  { get; set; } = string.Empty;
        public int     Quantite    { get; set; } = 1;
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO reçu par POST /api/it/demandesachat.
    /// Contient une liste de lignes de matériel.
    /// </summary>
    public class CreateDemandeAchatDto
    {
        /// <summary>Titre global de la demande (ex : "Équipements salle réunion").</summary>
        public string  NomProduit   { get; set; } = string.Empty;

        public string? Reference    { get; set; }
        public string? Description  { get; set; }
        public string? DemandeurNom { get; set; }

        /// <summary>Liste des lignes (au moins 1 obligatoire).</summary>
        public List<CreateLigneDemandeDto> Lignes { get; set; } = new();
    }

    // ── DTO vue IT (liste + création) ────────────────────────────

    public class DemandeAchatITDto
    {
        public int      IdDemande    { get; set; }
        public string   Reference    { get; set; } = string.Empty;
        public string   NomProduit   { get; set; } = string.Empty;
        public int      Quantite     { get; set; }
        public string?  Description  { get; set; }
        public string   Statut       { get; set; } = "en_attente";
        public DateTime DateCreation { get; set; }
        public string   DemandeurNom { get; set; } = string.Empty;
        public string?  MotifRefus   { get; set; }

        public List<LigneDemandeDto> Lignes { get; set; } = new();
    }

    // ── CHANGER STATUT ───────────────────────────────────────────

    public class ChangerStatutDto
    {
        public string  Statut     { get; set; } = string.Empty;
        public string? MotifRefus { get; set; }
    }

    // ── RÉPONSE STANDARD ────────────────────────────────────────

    public class DemandeAchatReponseDto
    {
        public bool   Succes    { get; set; }
        public string Message   { get; set; } = string.Empty;
        public int?   IdDemande { get; set; }
    }
}
