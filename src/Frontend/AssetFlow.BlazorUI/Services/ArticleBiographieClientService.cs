using AssetFlow.BlazorUI.DTOs;
using System.Net.Http.Json;

namespace AssetFlow.BlazorUI.Services
{
    public class ArticleBiographieClientService
    {
        private readonly HttpClient _http;

        public ArticleBiographieClientService(HttpClient http)
        {
            _http = http;
        }

        public Task<List<MaterielAvecArticlesDto>?> GetMaterielsAsync()
            => _http.GetFromJsonAsync<List<MaterielAvecArticlesDto>>("api/ArticleBiographie/materiels");

        public Task<ArticleBiographieDto?> GetBiographieAsync(int articleId)
            => _http.GetFromJsonAsync<ArticleBiographieDto>($"api/ArticleBiographie/{articleId}");
    }
}
