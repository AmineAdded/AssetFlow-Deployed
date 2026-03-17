// ============================================================
// AssetFlow.Application / DTOs / OffreSelectionDto.cs
// ============================================================

namespace AssetFlow.Application.DTOs
{
    /// <summary>Payload envoyé par le frontend lors du Confirmer.</summary>
    public class OffreSelectionDto
    {
        public string NomPdf  { get; set; } = string.Empty;
        public object Contenu { get; set; } = new();   // l'objet complet des champs OCR
        public string UserId  { get; set; } = string.Empty;
    }

    /// <summary>Contenu détaillé de l'offre (champs OCR + lignes).</summary>
    public class OffreContenuDto
    {
        public string FraisLivraison { get; set; } = string.Empty;
        public string DelaiLivraison { get; set; } = string.Empty;
        public string Garantie       { get; set; } = string.Empty;
        public string TotalHt        { get; set; } = string.Empty;
        public string TotalTva       { get; set; } = string.Empty;
        public string TotalTtc       { get; set; } = string.Empty;
        public List<LigneContenuDto> Lignes { get; set; } = new();
    }

    public class LigneContenuDto
    {
        public string Description    { get; set; } = string.Empty;
        public string Quantite       { get; set; } = string.Empty;
        public string Unite          { get; set; } = string.Empty;
        public string PrixUnitaireHt { get; set; } = string.Empty;
        public string TvaPct         { get; set; } = string.Empty;
        public string TotalTva       { get; set; } = string.Empty;
        public string TotalTtc       { get; set; } = string.Empty;
    }
}