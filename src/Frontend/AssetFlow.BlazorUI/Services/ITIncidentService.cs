using System.Net.Http.Json;
using AssetFlow.BlazorUI.DTOs;

namespace AssetFlow.BlazorUI.Services
{

    public class ITIncidentService
    {
        private readonly HttpClient _http;
        private const string Base = "api/it/incidents";

        public ITIncidentService(HttpClient http) => _http = http;

        public async Task<List<IncidentEmployeDto>> GetEmployesAsync(string? search = null)
        {
            var url = string.IsNullOrWhiteSpace(search) ? $"{Base}/employes"
                : $"{Base}/employes?search={Uri.EscapeDataString(search)}";
            try { return await _http.GetFromJsonAsync<List<IncidentEmployeDto>>(url) ?? new(); }
            catch { return new(); }
        }

        public async Task<List<IncidentMaterielDto>> GetMaterielsAsync(int userId)
        {
            try { return await _http.GetFromJsonAsync<List<IncidentMaterielDto>>($"{Base}/employes/{userId}/materiels") ?? new(); }
            catch { return new(); }
        }

        public async Task<(bool Ok, string Msg)> ChangerStatutAsync(int incidentId, string statut, string? commentaire = null)
        {
            try
            {
                var resp = await _http.PatchAsJsonAsync($"{Base}/{incidentId}/statut",
                    new { NouveauStatut = statut, CommentairesResolution = commentaire });
                var r = await resp.Content.ReadFromJsonAsync<SignalerIncidentResult>();
                return (r?.Success ?? false, r?.Message ?? "Erreur");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Ok, string Msg)> ResolveAllByArticleAsync(int articleId, string? commentaire = null)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync($"{Base}/resolve-all-article",
                    new { ArticleId = articleId, CommentairesResolution = commentaire });
                var r = await resp.Content.ReadFromJsonAsync<SignalerIncidentResult>();
                return (r?.Success ?? false, r?.Message ?? "Erreur");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }

    public class SignalerIncidentResult
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}