using System.Net.Http.Json;

namespace AssetFlow.BlazorUI.Services
{
    public class ChatMessageDto
    {
        public int      Id         { get; set; }
        public int      SenderId   { get; set; }
        public int      ReceiverId { get; set; }
        public string   Content    { get; set; } = string.Empty;
        public DateTime SentAt     { get; set; }
        public bool     IsRead     { get; set; }
    }

    public class ConversationSummaryDto
    {
        public int      OtherUserId     { get; set; }
        public string?  LastMessage     { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int      UnreadCount     { get; set; }
    }

    public class MessagerieService
    {
        private readonly HttpClient _http;

        public MessagerieService(HttpClient http) => _http = http;

        public async Task<List<ChatMessageDto>> GetHistoryAsync(int userId, int otherUserId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ChatMessageDto>>(
                    $"api/messages/{userId}/{otherUserId}") ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<ConversationSummaryDto>> GetConversationsAsync(int userId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ConversationSummaryDto>>(
                    $"api/messages/conversations/{userId}") ?? new();
            }
            catch { return new(); }
        }
    }
}