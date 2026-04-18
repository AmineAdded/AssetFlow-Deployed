namespace AssetFlow.BlazorUI.DTOs
{
    public class ConversationDto
    {
        public int       EmployeId        { get; set; }
        public string    FullName         { get; set; } = string.Empty;
        public string    Initials         { get; set; } = string.Empty;
        public string    Role             { get; set; } = string.Empty;

        public string?   LastMessage      { get; set; }
        public DateTime? LastMessageTime  { get; set; }
        public int       UnreadCount      { get; set; }
        public bool      IsOnline         { get; set; }
        public bool      IsTyping         { get; set; }
    }
}