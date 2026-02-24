// ============================================================
// COUCHE  : AssetFlow.Application
// FICHIER : DTOs/FournisseurDtos.cs
// RÔLE    : Objets de transfert entre le Controller et le Frontend.
//           Même pattern que AuthDtos.cs, EmployeDtos.cs, IncidentDtos.cs
// ============================================================

namespace AssetFlow.Application.DTOs
{
    // ─────────────────────────────
    // DTO LECTURE (GET)
    // ─────────────────────────────
    public class FournisseurDto
    {
        public int IdFournisseur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Adresse { get; set; }
        public string? Mail { get; set; }

        public int CommandesTotales { get; set; }
        public decimal TauxLivraisonATemps { get; set; }
        public decimal ScoreFiabilite { get; set; }
        public DateTime? DerniereCommande { get; set; }
    }

    // ─────────────────────────────
    // DTO CREATION (POST)
    // ─────────────────────────────
    public class CreerFournisseurDto
    {
        public string Nom { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Adresse { get; set; }
        public string? Mail { get; set; }

        public int CommandesTotales { get; set; }
        public decimal TauxLivraisonATemps { get; set; }
        public decimal ScoreFiabilite { get; set; }
        public DateTime? DerniereCommande { get; set; }
    }

    // ─────────────────────────────
    // DTO MODIFICATION (PUT)
    // ─────────────────────────────
    public class ModifierFournisseurDto
    {
        public int IdFournisseur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Adresse { get; set; }
        public string? Mail { get; set; }

        public int CommandesTotales { get; set; }
        public decimal TauxLivraisonATemps { get; set; }
        public decimal ScoreFiabilite { get; set; }
        public DateTime? DerniereCommande { get; set; }
    }

    // ─────────────────────────────
    // DTO REPONSE
    // ─────────────────────────────
    public class FournisseurReponseDto
    {
        public bool Succes { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? IdFournisseur { get; set; }
    }
}
