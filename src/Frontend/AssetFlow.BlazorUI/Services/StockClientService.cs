using System.Net.Http.Json;
using AssetFlow.BlazorUI.DTOs;
namespace AssetFlow.BlazorUI.Services
{
    public class StockClientService
    {
        private readonly HttpClient _http;
        private const string Base = "api/it/stock"; 

        public StockClientService(HttpClient http) => _http = http;

        public async Task<List<MaterielDto>> GetAllAsync()
        {
            try { return await _http.GetFromJsonAsync<List<MaterielDto>>(Base) ?? new(); }
            catch { return new(); }
        }

       public async Task<bool> UpdateSeuilAsync(int id, int seuilMin)
        {
            try
            {
                var resp = await _http.PatchAsJsonAsync(
                    $"api/it/stock/{id}/seuil",
                    new { SeuilMin = seuilMin, SeuilCritique = 0 });
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}