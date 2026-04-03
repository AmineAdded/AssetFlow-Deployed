// Services/VoiceNluService.cs
using AssetFlow.BlazorUI.DTOs;
using System.Net.Http.Json;

namespace AssetFlow.BlazorUI.Services
{
    public class VoiceNluService
    {
        private readonly HttpClient _http;

        public VoiceNluService(HttpClient http) => _http = http;

        public async Task<VoiceCommandResponse?> ProcessAsync(
            string audioBase64, string mimeType, string role)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/voice/process", new
                {
                    audioBase64,
                    mimeType,
                    role
                });

                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<VoiceCommandResponse>();
            }
            catch
            {
                return null;
            }
        }
    }
}