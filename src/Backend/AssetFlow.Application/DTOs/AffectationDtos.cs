// ============================================================
// AssetFlow.Application / DTOs / AffectationDtos.cs
// DTOs pour les opérations d'affectation de matériel
// ============================================================

namespace AssetFlow.Application.DTOs
{
    /// <summary>
    /// DTO représentant un utilisateur disponible pour une affectation
    /// </summary>
    public class UtilisateurDisponibleDto
    {
        public int    Id         { get; set; }
        public string FullName   { get; set; } = string.Empty;
        public string Email      { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Initials   { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO représentant un matériel disponible pour une affectation
    /// </summary>
    public class MaterielDisponibleDto
    {
        public int    Id           { get; set; }
        public string Reference    { get; set; } = string.Empty;
        public string Designation  { get; set; } = string.Empty;
        public string Categorie    { get; set; } = string.Empty;
        public string? ImageUrl    { get; set; }
        public int    QuantiteDisponible { get; set; }

        /// <summary>Articles disponibles (Statut == Disponible)</summary>
        public List<ArticleDisponibleDto> Articles { get; set; } = new();
    }

    /// <summary>
    /// DTO représentant un article individuel disponible
    /// </summary>
    public class ArticleDisponibleDto
    {
        public int    Id          { get; set; }
        public string NumeroSerie { get; set; } = string.Empty;
        public string Etat        { get; set; } = "Bon";
    }

    /// <summary>
    /// Requête pour créer une affectation
    /// </summary>
    public class CreerAffectationDto
    {
        public int    MaterielId    { get; set; }
        public int    UtilisateurId { get; set; }

        /// <summary>IDs des articles individuels à affecter</summary>
        public List<int> ArticleIds { get; set; } = new();

        public string? Observations { get; set; }
        public DateTime? DateRetourPrevue { get; set; }
    }

    /// <summary>
    /// Résultat de la création d'une affectation
    /// </summary>
    public class AffectationResultDto
    {
        public bool   Succes        { get; set; }
        public string Message       { get; set; } = string.Empty;
        public int    AffectationId { get; set; }
    }
}