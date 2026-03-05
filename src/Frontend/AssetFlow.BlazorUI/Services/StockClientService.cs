using System.Net.Http.Json;

namespace AssetFlow.BlazorUI.Services
{
    public class MaterielDto
    {
        public int      Id            { get; set; }
        public string   Reference     { get; set; } = string.Empty;
        public string   Designation   { get; set; } = string.Empty;
        public string?  Description   { get; set; }
        public string   Categorie     { get; set; } = string.Empty;
        public int      QuantiteStock { get; set; }
        public int      QuantiteMin   { get; set; }
        public string   Unite         { get; set; } = "pièce";
        public string?  Emplacement   { get; set; }
        public string?  ImageUrl      { get; set; }
        public DateTime DateAjout     { get; set; }
    }

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