namespace AssetFlow.BlazorUI.DTOs
{
    public class ConversationSummaryDto
    {
        public int      OtherUserId     { get; set; }
        public string?  LastMessage     { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int      UnreadCount     { get; set; }
    }
}