// ============================================================
// COUCHE  : AssetFlow.Domain
// FICHIER : Entities/Fournisseur.cs
// RÔLE    : Entité métier pure qui reflète exactement la table SQL.
//           Aucune dépendance externe (pas d'EF Core, pas de HTTP).
//
// TABLE SQL :
//   CREATE TABLE Fournisseur (
//       IdFournisseur INT IDENTITY(1,1) PRIMARY KEY,
//       Nom        VARCHAR(100) NOT NULL,
//       Telephone  VARCHAR(20),
//       Adresse    VARCHAR(255),
//       Mail       VARCHAR(150)
//   );
// ============================================================

namespace AssetFlow.Domain.Entities
{
    /// <summary>
    /// Représente un fournisseur dans le système AssetFlow.
    /// Les propriétés correspondent exactement aux colonnes de la table SQL.
    /// </summary>
    public class Fournisseur
    {
        /// <summary>Clé primaire — INT IDENTITY(1,1)</summary>
        public int IdFournisseur { get; set; }

        /// <summary>Nom du fournisseur — VARCHAR(100) NOT NULL</summary>
        public string Nom { get; set; } = string.Empty;

        /// <summary>Numéro de téléphone — VARCHAR(20) nullable</summary>
        public string? Telephone { get; set; }

        /// <summary>Adresse physique — VARCHAR(255) nullable</summary>
        public string? Adresse { get; set; }

        /// <summary>Adresse e-mail — VARCHAR(150) nullable</summary>
        public string? Mail { get; set; }
    }
}
