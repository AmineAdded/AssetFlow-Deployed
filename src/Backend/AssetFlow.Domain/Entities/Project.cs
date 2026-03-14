// ============================================================
// AssetFlow.Domain / Entities / Project.cs
// ============================================================

namespace AssetFlow.Domain.Entities
{
    public class Project
    {
        public int Id { get; set; }

        /// <summary>Nom du projet</summary>
        public string Nom { get; set; } = string.Empty;

        /// <summary>Description détaillée</summary>
        public string? Description { get; set; }

        /// <summary>Statut : EnCours, Termine, Suspendu, Planifie</summary>
        public StatutProjet Statut { get; set; } = StatutProjet.Planifie;

        /// <summary>Priorité : Faible, Moyenne, Haute, Critique</summary>
        public PrioriteProjet Priorite { get; set; } = PrioriteProjet.Moyenne;

        /// <summary>Responsable du projet</summary>
        public string? Responsable { get; set; }

        /// <summary>Budget alloué</summary>
        public decimal? Budget { get; set; }

        /// <summary>Date de début prévue</summary>
        public DateTime? DateDebut { get; set; }

        /// <summary>Date de fin prévue</summary>
        public DateTime? DateFin { get; set; }

        /// <summary>Date de création de la fiche</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Dernière modification</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum StatutProjet
    {
        Planifie  = 0,
        EnCours   = 1,
        Suspendu  = 2,
        Termine   = 3
    }

    public enum PrioriteProjet
    {
        Faible   = 0,
        Moyenne  = 1,
        Haute    = 2,
        Critique = 3
    }
}