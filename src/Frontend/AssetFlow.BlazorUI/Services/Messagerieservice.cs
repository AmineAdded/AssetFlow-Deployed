using System.Net.Http.Json;
using AssetFlow.BlazorUI.DTOs;

namespace AssetFlow.BlazorUI.Services
{
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