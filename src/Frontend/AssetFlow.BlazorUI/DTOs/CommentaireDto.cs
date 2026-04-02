namespace AssetFlow.BlazorUI.DTOs
{
    public class CommentaireDto
    {
        public int      Id              { get; set; }
        public int      MaterielId      { get; set; }
        public int      UtilisateurId   { get; set; }
        public string   AuteurNom       { get; set; } = string.Empty;
        public string   AuteurInitiales { get; set; } = string.Empty;
        public string   Contenu         { get; set; } = string.Empty;
        public DateTime DateCreation    { get; set; }
    }

    public class CreerCommentaireDto
    {
        public int    MaterielId    { get; set; }
        public int    UtilisateurId { get; set; }
        public string Contenu       { get; set; } = string.Empty;
    }

    public class CommentaireResultDto
    {
        public bool   Succes  { get; set; }
        public string Message { get; set; } = string.Empty;
        public int?   Id      { get; set; }
    }

        public class CommentaireITDto
        {
            public int      Id                { get; set; }
            public int      MaterielId        { get; set; }
            public string   MaterielRef       { get; set; } = string.Empty;
            public string   MaterielNom       { get; set; } = string.Empty;
            public string   MaterielCategorie { get; set; } = string.Empty;
            public int      UtilisateurId     { get; set; }
            public string   AuteurNom         { get; set; } = string.Empty;
            public string   AuteurInitiales   { get; set; } = string.Empty;
            public string   AuteurRole        { get; set; } = string.Empty;
            public string   Contenu           { get; set; } = string.Empty;
            public DateTime DateCreation      { get; set; }
        }

        public class SentimentMaterielDto
        {
            public int    MaterielId          { get; set; }
            public string MaterielRef         { get; set; } = string.Empty;
            public string MaterielNom         { get; set; } = string.Empty;
            public int    TotalCommentaires   { get; set; }
            public int    Positifs            { get; set; }
            public int    Negatifs            { get; set; }
            public int    Neutres             { get; set; }
            public double PourcentagePositif  { get; set; }
            public double PourcentageNegatif  { get; set; }
            public double PourcentageNeutre   { get; set; }
            public string Resume              { get; set; } = string.Empty;
            public double ScoreGlobal         { get; set; }
            public string SentimentDominant   { get; set; } = string.Empty;
        }
}