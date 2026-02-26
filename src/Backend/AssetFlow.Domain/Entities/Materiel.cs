// ============================================================
// AssetFlow.Domain / Entities / Materiel.cs
// Entité représentant un matériel dans le système
// ============================================================

namespace AssetFlow.Domain.Entities
{
    /// <summary>
    /// Représente un équipement/matériel géré dans AssetFlow
    /// </summary>
    public class Materiel
    {
        /// <summary>Identifiant unique</summary>
        public int Id { get; set; }

        /// <summary>Référence unique du matériel (ex: SN-5592-X)</summary>
        public string Reference { get; set; } = string.Empty;

        /// <summary>Nom/désignation du matériel (ex: Laptop Dell Latitude)</summary>
        public string Designation { get; set; } = string.Empty;

        /// <summary>Description détaillée</summary>
        public string? Description { get; set; }

        /// <summary>Catégorie (ex: Ordinateur, Périphérique, Mobilier)</summary>
        public string Categorie { get; set; } = string.Empty;

        /// <summary>Quantité disponible en stock</summary>
        public int QuantiteStock { get; set; }

        /// <summary>Quantité minimale avant alerte</summary>
        public int QuantiteMin { get; set; }

        /// <summary>Unité de mesure (pièce, lot, etc.)</summary>
        public string Unite { get; set; } = "pièce";

        /// <summary>Emplacement physique du stock</summary>
        public string? Emplacement { get; set; }

        /// <summary>URL de l'image du matériel (stockée localement)</summary>
        public string? ImageUrl { get; set; }

        /// <summary>Date d'ajout au système</summary>
        public DateTime DateAjout { get; set; } = DateTime.UtcNow;

        /// <summary>Navigation : affectations de ce matériel</summary>
        public ICollection<Affectation> Affectations { get; set; } = new List<Affectation>();
    }
}