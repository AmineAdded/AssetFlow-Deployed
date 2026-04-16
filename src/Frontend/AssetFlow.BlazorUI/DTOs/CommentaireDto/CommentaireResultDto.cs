namespace AssetFlow.BlazorUI.DTOs
{
    public class CommentaireResultDto
    {
        public bool   Succes  { get; set; }
        public string Message { get; set; } = string.Empty;
        public int?   Id      { get; set; }
    }
}