namespace AssetFlow.BlazorUI.DTOs
{
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