using System.Net.Http.Json;
using AssetFlow.BlazorUI.DTOs;

namespace AssetFlow.BlazorUI.Services
{

    public class AffectationClientService
    {
        private readonly HttpClient _http;
        private const string Base = "api/affectation";

        public AffectationClientService(HttpClient http) => _http = http;

        public async Task<List<UtilisateurDisponibleDto>> GetUtilisateursAsync(string? search = null)
        {
            var url = string.IsNullOrWhiteSpace(search)
                ? $"{Base}/utilisateurs"
                : $"{Base}/utilisateurs?search={Uri.EscapeDataString(search)}";
            try { return await _http.GetFromJsonAsync<List<UtilisateurDisponibleDto>>(url) ?? new(); }
            catch { return new(); }
        }

        public async Task<List<MaterielDisponibleDto>> GetMaterielsAsync(string? search = null)
        {
            var url = string.IsNullOrWhiteSpace(search)
                ? $"{Base}/materiels"
                : $"{Base}/materiels?search={Uri.EscapeDataString(search)}";
            try { return await _http.GetFromJsonAsync<List<MaterielDisponibleDto>>(url) ?? new(); }
            catch { return new(); }
        }

        public async Task<List<ProjetDisponibleDto>> GetProjetsAsync(string? search = null)
        {
            var url = string.IsNullOrWhiteSpace(search)
                ? $"{Base}/projets"
                : $"{Base}/projets?search={Uri.EscapeDataString(search)}";
            try { return await _http.GetFromJsonAsync<List<ProjetDisponibleDto>>(url) ?? new(); }
            catch { return new(); }
        }

        public async Task<AffectationResultDto> CreerAffectationAsync(CreerAffectationRequest request)
        {
            try
            {
                var resp   = await _http.PostAsJsonAsync(Base, request);
                var result = await resp.Content.ReadFromJsonAsync<AffectationResultDto>();
                return result ?? new AffectationResultDto { Succes = false, Message = "Réponse vide." };
            }
            catch (Exception ex)
            {
                return new AffectationResultDto { Succes = false, Message = $"Erreur réseau: {ex.Message}" };
            }
        }
    }
}