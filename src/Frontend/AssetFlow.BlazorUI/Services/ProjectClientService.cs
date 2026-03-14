// ============================================================
// AssetFlow.BlazorUI / Services / ProjectClientService.cs
// Service HTTP côté Blazor pour les projets
// ============================================================

using System.Net.Http.Json;

namespace AssetFlow.BlazorUI.Services
{
    public class ProjectClientService
    {
        private readonly HttpClient _http;
        public ProjectClientService(HttpClient http) => _http = http;

        public Task<List<ProjectDto>?> GetAllAsync()
            => _http.GetFromJsonAsync<List<ProjectDto>>("api/projects");

        public Task<HttpResponseMessage> CreateAsync(object dto)
            => _http.PostAsJsonAsync("api/projects", dto);

        public Task<HttpResponseMessage> UpdateAsync(int id, object dto)
            => _http.PutAsJsonAsync($"api/projects/{id}", dto);

        public Task<HttpResponseMessage> DeleteAsync(int id)
            => _http.DeleteAsync($"api/projects/{id}");
    }

    // ── DTO partagé Blazor (copie légère de Application.DTOs.ProjectDto) ──
    public class ProjectDto
    {
        public int       Id          { get; set; }
        public string    Nom         { get; set; } = string.Empty;
        public string?   Description { get; set; }
        public string    Statut      { get; set; } = "Planifie";
        public string    Priorite    { get; set; } = "Moyenne";
        public string?   Responsable { get; set; }
        public decimal?  Budget      { get; set; }
        public DateTime? DateDebut   { get; set; }
        public DateTime? DateFin     { get; set; }
        public DateTime  CreatedAt   { get; set; }
        public DateTime  UpdatedAt   { get; set; }
    }
}