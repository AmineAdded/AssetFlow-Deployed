namespace AssetFlow.BlazorUI.DTOs
{
    public class ITUserSimpleDto
    {
        public int    Id       { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
    }

    // Liste des IT pour la messagerie chez Achat
    public class ITSimpleDto
    {
        public int    Id       { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
    }
}