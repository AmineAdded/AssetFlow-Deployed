// ============================================================
// AssetFlow.Domain / Entities / Affectation.cs
// Entité représentant l'affectation d'un matériel à un employé
// ============================================================

namespace AssetFlow.Domain.Entities
{
    /// <summary>
    /// Représente l'affectation d'un matériel à un utilisateur
    /// </summary>
    public class Affectation
    {
        /// <summary>Identifiant unique</summary>
        public int Id { get; set; }

        /// <summary>Date de l'affectation</summary>
        public DateTime DateAffectation { get; set; } = DateTime.UtcNow;

        /// <summary>Quantité affectée</summary>
        public int QuantiteAffectee { get; set; }

        /// <summary>Quantité retournée (si retour partiel)</summary>
        public int QuantiteRetournee { get; set; } = 0;

        /// <summary>Date de retour effectif (null si toujours affecté)</summary>
        public DateTime? DateRetour { get; set; }

        /// <summary>Observations ou notes sur l'affectation</summary>
        public string? Observations { get; set; }

        // === RELATIONS ===

        /// <summary>ID du matériel affecté</summary>
        public int MaterielId { get; set; }

        /// <summary>Navigation : matériel affecté</summary>
        public Materiel Materiel { get; set; } = null!;

        /// <summary>ID de l'utilisateur qui reçoit le matériel</summary>
        public int UtilisateurId { get; set; }

        /// <summary>Navigation : utilisateur qui reçoit le matériel</summary>
        public User Utilisateur { get; set; } = null!;
        public List<ArticleIndividuel> Articles { get; set; } = new();
        public EtatAffectation Etat { get; set; } = EtatAffectation.Courante;
    }

    public enum EtatAffectation  // ← AJOUTER (à la fin du fichier)
    {
        Courante  = 0,
        Terminee  = 1
    }
}