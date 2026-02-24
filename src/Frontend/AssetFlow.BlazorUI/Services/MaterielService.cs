// ============================================================
// AssetFlow.BlazorUI / Services / MaterielService.cs
// Client HTTP pour l'API Matériel — utilisé par les pages Blazor
// ============================================================

using System.Net.Http.Json;
using AssetFlow.Application.DTOs;
using AssetFlow.Application.Interfaces;

namespace AssetFlow.BlazorUI.Services
{
    /// <summary>
    /// Service Blazor communiquant avec /api/materiel via HttpClient
    /// </summary>
    public class MaterielService
    {
        private readonly HttpClient _http;

        // URL de base de l'API (sans slash final)
        private const string Base = "api/materiel";

        public MaterielService(HttpClient http) => _http = http;

        // ── Lecture ───────────────────────────────────────────────

        /// <summary>Charge tous les matériels</summary>
        public async Task<List<MaterielDto>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<MaterielDto>>(Base);
            return result ?? new();
        }

        /// <summary>Charge un matériel par son id</summary>
        public async Task<MaterielDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<MaterielDto>($"{Base}/{id}");

        /// <summary>Recherche avec filtres optionnels</summary>
        public async Task<List<MaterielDto>> SearchAsync(
            string? terme = null, string? categorie = null, string? etat = null)
        {
            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(terme))      qs.Add($"terme={Uri.EscapeDataString(terme)}");
            if (!string.IsNullOrWhiteSpace(categorie))  qs.Add($"categorie={Uri.EscapeDataString(categorie)}");
            if (!string.IsNullOrWhiteSpace(etat))       qs.Add($"etat={Uri.EscapeDataString(etat)}");

            var url = qs.Count > 0 ? $"{Base}/search?{string.Join("&", qs)}" : $"{Base}/search";
            var result = await _http.GetFromJsonAsync<List<MaterielDto>>(url);
            return result ?? new();
        }

        /// <summary>Charge les stats (KPIs)</summary>
        public async Task<MaterielStatsDto?> GetStatsAsync()
            => await _http.GetFromJsonAsync<MaterielStatsDto>($"{Base}/stats");

        // ── Écriture ──────────────────────────────────────────────

        /// <summary>Crée un nouveau matériel</summary>
        public async Task<MaterielResultDto> AjouterAsync(CreerMaterielDto dto)
        {
            var resp = await _http.PostAsJsonAsync(Base, dto);
            return await resp.Content.ReadFromJsonAsync<MaterielResultDto>()
                   ?? new() { Succes = false, Message = "Réponse vide du serveur." };
        }

        /// <summary>Met à jour un matériel</summary>
        public async Task<MaterielResultDto> ModifierAsync(ModifierMaterielDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"{Base}/{dto.Id}", dto);
            return await resp.Content.ReadFromJsonAsync<MaterielResultDto>()
                   ?? new() { Succes = false, Message = "Réponse vide du serveur." };
        }

        /// <summary>Supprime un matériel par son id</summary>
        public async Task<MaterielResultDto> SupprimerAsync(int id)
        {
            var resp = await _http.DeleteAsync($"{Base}/{id}");
            return await resp.Content.ReadFromJsonAsync<MaterielResultDto>()
                   ?? new() { Succes = false, Message = "Réponse vide du serveur." };
        }
    }
}