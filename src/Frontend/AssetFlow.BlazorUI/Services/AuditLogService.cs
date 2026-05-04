using System.Net.Http.Json;
using System.Text;
using AssetFlow.BlazorUI.DTOs;

namespace AssetFlow.BlazorUI.Services
{
    public class AuditLogService
    {
        private readonly HttpClient _http;
        public AuditLogService(HttpClient http) => _http = http;

        public async Task<AuditLogPagedDto?> GetLogsAsync(
            DateTime? dateDebut   = null,
            DateTime? dateFin     = null,
            string?   utilisateur = null,
            string?   action      = null,
            string?   categorie   = null,
            string?   search      = null,
            int       page        = 1,
            int       pageSize    = 50)
        {
            var sb = new StringBuilder($"api/audit-logs?page={page}&pageSize={pageSize}");

            if (dateDebut.HasValue)
                sb.Append($"&dateDebut={dateDebut.Value:yyyy-MM-dd}");
            if (dateFin.HasValue)
                sb.Append($"&dateFin={dateFin.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(utilisateur) && utilisateur != "Tous les utilisateurs")
                sb.Append($"&utilisateur={Uri.EscapeDataString(utilisateur)}");
            if (!string.IsNullOrWhiteSpace(action) && action != "Toutes les actions")
                sb.Append($"&action={Uri.EscapeDataString(action)}");
            if (!string.IsNullOrWhiteSpace(categorie) && categorie != "Toutes")
                sb.Append($"&categorie={Uri.EscapeDataString(categorie)}");
            if (!string.IsNullOrWhiteSpace(search))
                sb.Append($"&search={Uri.EscapeDataString(search)}");

            try
            {
                return await _http.GetFromJsonAsync<AuditLogPagedDto>(sb.ToString());
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuditLogStatsDto?> GetStatsAsync()
        {
            try { return await _http.GetFromJsonAsync<AuditLogStatsDto>("api/audit-logs/stats"); }
            catch { return null; }
        }

        public async Task<(bool success, string message)> SupprimerAvantDateAsync(DateTime date)
        {
            try
            {
                var res = await _http.DeleteAsync(
                    $"api/audit-logs/avant-date?date={date:yyyy-MM-dd}");
                var data = await res.Content.ReadFromJsonAsync<DeleteResultDto>();
                return (res.IsSuccessStatusCode, data?.Message ?? "Erreur");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> SupprimerParCategorieAsync(string categorie)
        {
            try
            {
                var res = await _http.DeleteAsync(
                    $"api/audit-logs/par-categorie?categorie={Uri.EscapeDataString(categorie)}");
                var data = await res.Content.ReadFromJsonAsync<DeleteResultDto>();
                return (res.IsSuccessStatusCode, data?.Message ?? "Erreur");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> SupprimerToutAsync()
        {
            try
            {
                var res  = await _http.DeleteAsync("api/audit-logs/tout");
                var data = await res.Content.ReadFromJsonAsync<DeleteResultDto>();
                return (res.IsSuccessStatusCode, data?.Message ?? "Erreur");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // DTO helper
        private record DeleteResultDto(
            [property: System.Text.Json.Serialization.JsonPropertyName("supprimés")] int Supprimes,
            [property: System.Text.Json.Serialization.JsonPropertyName("message")]   string Message);
        }
}