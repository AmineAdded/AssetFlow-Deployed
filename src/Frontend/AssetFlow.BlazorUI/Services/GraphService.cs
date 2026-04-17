using AssetFlow.BlazorUI.DTOs;
using System.Net.Http.Json;

namespace AssetFlow.BlazorUI.Services
{
    public class GraphService
    {
        private readonly HttpClient _http;

        public GraphService(HttpClient http)
        {
            _http = http;
        }

        public async Task<GraphResponseDto?> GetGraphAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<GraphResponseDto>("api/graph");
            }
            catch
            {
                return null;
            }
        }

        public async Task<GraphInsightDto?> GetNodeInsightAsync(string nodeId)
        {
            try
            {
                return await _http.GetFromJsonAsync<GraphInsightDto>($"api/graph/insight/{nodeId}");
            }
            catch
            {
                return null;
            }
        }
    }
}