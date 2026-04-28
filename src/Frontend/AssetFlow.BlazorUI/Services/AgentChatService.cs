// src/Frontend/AssetFlow.BlazorUI/Services/AgentService.cs
using System.Net.Http.Json;
using AssetFlow.BlazorUI.DTOs;

namespace AssetFlow.BlazorUI.Services
{
    public class AgentChatService
    {
        private readonly HttpClient _http;
        private const string Base = "api/agent";

        public AgentChatService(HttpClient http) => _http = http;

        public async Task<AgentChatResponse?> GetInitialAlertsAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<AgentChatResponse>($"{Base}/alerts");
            }
            catch { return null; }
        }

        public async Task<AgentChatResponse?> ChatAsync(AgentChatRequest request)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync($"{Base}/chat", request);
                return await resp.Content.ReadFromJsonAsync<AgentChatResponse>();
            }
            catch { return null; }
        }

        public async Task<AgentApprovalResponse?> ApproveAsync(AgentApprovalRequest request, string userName)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, $"{Base}/approve");
                req.Headers.Add("X-User-Name", userName);
                req.Content = JsonContent.Create(request);
                var resp = await _http.SendAsync(req);
                return await resp.Content.ReadFromJsonAsync<AgentApprovalResponse>();
            }
            catch { return null; }
        }
    }
}
