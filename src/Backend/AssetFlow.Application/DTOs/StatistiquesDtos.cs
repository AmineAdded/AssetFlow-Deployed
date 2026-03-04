// ============================================================
// AssetFlow.Application / DTOs / StatistiquesDtos.cs — v2
// Inclut des méthodes de filtrage côté client (Blazor)
// pour éviter des appels API supplémentaires
// ============================================================

using System.Globalization;

namespace AssetFlow.Application.DTOs
{
    // ── DTOs de base ─────────────────────────────────────────

    public class EtatDemandesDto
    {
        public int EnAttente { get; set; }
        public int Commande  { get; set; }
        public int Traite    { get; set; }
        public int Refuse    { get; set; }
    }

    public class DemandesParSemaineDto
    {
        public string Label     { get; set; } = string.Empty;
        public int    EnAttente { get; set; }
        public int    Commande  { get; set; }
        public int    Traite    { get; set; }
    }

    public class AffectationMaterielDto
    {
        public int Affecte    { get; set; }
        public int NonAffecte { get; set; }
    }

    public class ArticlesParMaterielDto
    {
        public string Designation  { get; set; } = string.Empty;
        public int    Disponibles  { get; set; }
        public int    Affectes     { get; set; }
        public int    HorsService  { get; set; }
        public int    EnReparation { get; set; }
    }

    /// <summary>Articles individuels groupés par catégorie de matériel</summary>
    public class ArticlesParCategorieDto
    {
        public string Categorie   { get; set; } = string.Empty;
        public int    Disponibles { get; set; }
        public int    Affectes    { get; set; }
        public int    HorsService { get; set; }
        public int    EnReparation{ get; set; }
    }

    /// <summary>Point brut d'une demande — pour filtrage client</summary>
    public class DemandeRawDto
    {
        public DateTime DateCreation { get; set; }
        public string   Statut       { get; set; } = string.Empty;
    }

    // ── DTO global retourné par l'API ─────────────────────────

    public class DashboardStatsDto
    {
        // KPIs
        public int TotalMateriels       { get; set; }
        public int TotalCommandes       { get; set; }
        public int TotalArticles        { get; set; }
        public int TotalDemandesActives { get; set; }

        // Données pour graphes
        public AffectationMaterielDto         AffectationMateriel  { get; set; } = new();
        public List<ArticlesParMaterielDto>   ArticlesParMateriel  { get; set; } = new();
        public List<ArticlesParCategorieDto>  ArticlesParCategorie { get; set; } = new();

        /// <summary>Toutes les demandes brutes pour filtrage côté client</summary>
        public List<DemandeRawDto>            DemandesRaw          { get; set; } = new();

        // ── Méthodes de filtrage client ───────────────────────

        /// <summary>Calcule la répartition des statuts selon année et mois</summary>
        public EtatDemandesDto GetEtatDemandes(int annee, int mois)
        {
            var q = DemandesRaw.Where(d => d.DateCreation.Year == annee);
            if (mois > 0)
                q = q.Where(d => d.DateCreation.Month == mois);
            var list = q.ToList();
            return new EtatDemandesDto
            {
                EnAttente = list.Count(d => d.Statut == "en_attente"),
                Commande  = list.Count(d => d.Statut == "commande"),
                Traite    = list.Count(d => d.Statut == "traite"),
                Refuse    = list.Count(d => d.Statut == "refuse"),
            };
        }

        /// <summary>
        /// Retourne N semaines entre dateDebut et dateFin (de la plus ancienne à la plus récente).
        /// Si la plage couvre plus de N semaines, on prend les N dernières.
        /// </summary>
        public List<DemandesParSemaineDto> GetDemandesParSemaine(
            DateTime debut, DateTime fin, int nbSemaines = 8)
        {
            // Ajuster si la plage est trop grande : garder les N dernières semaines
            var realDebut = fin.AddDays(-(nbSemaines * 7 - 1));
            if (debut > realDebut) realDebut = debut;

            var result = new List<DemandesParSemaineDto>();
            var cursor = realDebut.Date;

            for (int i = 0; i < nbSemaines; i++)
            {
                var weekStart = cursor;
                var weekEnd   = cursor.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
                if (weekStart > fin.Date) break;

                var sem  = DemandesRaw
                    .Where(d => d.DateCreation.Date >= weekStart &&
                                d.DateCreation.Date <= weekEnd.Date)
                    .ToList();

                int wn = ISOWeek.GetWeekOfYear(weekStart);
                result.Add(new DemandesParSemaineDto
                {
                    Label     = $"S{wn:D2}",
                    EnAttente = sem.Count(d => d.Statut == "en_attente"),
                    Commande  = sem.Count(d => d.Statut == "commande"),
                    Traite    = sem.Count(d => d.Statut == "traite"),
                });

                cursor = cursor.AddDays(7);
            }

            return result;
        }

        /// <summary>
        /// Retourne les 4 semaines d'un mois donné.
        /// S1 = jours 1-7, S2 = 8-14, S3 = 15-21, S4 = 22-fin
        /// </summary>
        public List<DemandesParSemaineDto> GetDemandesSemaineDuMois(int annee, int mois)
        {
            var result = new List<DemandesParSemaineDto>();
            var ranges = new[]
            {
                (1,  7,  "Semaine 1"),
                (8,  14, "Semaine 2"),
                (15, 21, "Semaine 3"),
                (22, DateTime.DaysInMonth(annee, mois), "Semaine 4"),
            };

            foreach (var (jourDebut, jourFin, label) in ranges)
            {
                var d1 = new DateTime(annee, mois, jourDebut);
                var d2 = new DateTime(annee, mois, Math.Min(jourFin, DateTime.DaysInMonth(annee, mois)));

                var sem = DemandesRaw
                    .Where(d => d.DateCreation.Date >= d1 && d.DateCreation.Date <= d2)
                    .ToList();

                result.Add(new DemandesParSemaineDto
                {
                    Label     = label,
                    EnAttente = sem.Count(d => d.Statut == "en_attente"),
                    Commande  = sem.Count(d => d.Statut == "commande"),
                    Traite    = sem.Count(d => d.Statut == "traite"),
                });
            }
            return result;
        }
    }
}
