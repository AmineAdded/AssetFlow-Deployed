namespace AssetFlow.Application.DTOs
{
    public class EmployeListeDto
    {
        public int    Id         { get; set; }
        public string FullName   { get; set; } = string.Empty;
        public string Email      { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Initials   { get; set; } = string.Empty;
        public int    NbAffectationsActives { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AffectationEmployeDto
    {
        public int      AffectationId    { get; set; }
        public int      MaterielId       { get; set; }
        public string   Designation      { get; set; } = string.Empty;
        public string   Reference        { get; set; } = string.Empty;
        public string   Categorie        { get; set; } = string.Empty;
        public string?  ImageUrl         { get; set; }
        public DateTime DateAffectation  { get; set; }
        public DateTime? DateRetourPrevue { get; set; }
        public string   Etat             { get; set; } = string.Empty;
        public string?  Observations     { get; set; }
        public List<ArticleAffectationDto> Articles { get; set; } = new();
    }

    public class ArticleAffectationDto
    {
        public int    ArticleId    { get; set; }
        public string NumeroSerie  { get; set; } = string.Empty;
        public string Etat         { get; set; } = string.Empty;
    }

    public class RetirerAffectationResultDto
    {
        public bool   Succes  { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}