// ============================================================
// COUCHE  : AssetFlow.BlazorUI (Frontend)
// FICHIER : Services/FournisseurService.cs
// RÔLE    : Appelle l'API REST via HttpClient.
//           Même structure qu'AuthService.cs, EmployeService.cs, IncidentService.cs.
//           Enregistré dans Program.cs (BlazorUI) comme service Scoped.
// ============================================================

using System.Net.Http.Json;
using AssetFlow.Application.DTOs;

namespace AssetFlow.BlazorUI.Services
{
    /// <summary>
    /// Service Blazor pour les appels HTTP vers /api/fournisseurs.
    /// Injecté dans les pages Razor avec @inject FournisseurService FournisseurSvc.
    /// </summary>
    public class FournisseurService
    {
        private readonly HttpClient _http;

        public FournisseurService(HttpClient http)
        {
            _http = http;
        }

        // ────────────────────────────────────────────────────────
        // GET ALL
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Appelle GET /api/fournisseurs.
        /// Retourne liste vide si erreur réseau.
        /// </summary>
        public async Task<List<FournisseurDto>> GetAllAsync()
        {
            try
            {
                var result = await _http
                    .GetFromJsonAsync<List<FournisseurDto>>("api/fournisseurs");
                return result ?? new List<FournisseurDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FournisseurService.GetAll] {ex.Message}");
                return new List<FournisseurDto>();
            }
        }

        // ────────────────────────────────────────────────────────
        // GET BY ID
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Appelle GET /api/fournisseurs/{id}.
        /// Retourne null si non trouvé.
        /// </summary>
        public async Task<FournisseurDto?> GetByIdAsync(int id)
        {
            try
            {
                return await _http
                    .GetFromJsonAsync<FournisseurDto>($"api/fournisseurs/{id}");
            }
            catch
            {
                return null;
            }
        }

        // ────────────────────────────────────────────────────────
        // RECHERCHER
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Appelle GET /api/fournisseurs/recherche?terme=xxx.
        /// Retourne liste vide si aucun résultat.
        /// </summary>
        public async Task<List<FournisseurDto>> RechercherAsync(string terme)
        {
            try
            {
                var enc = Uri.EscapeDataString(terme);
                var result = await _http
                    .GetFromJsonAsync<List<FournisseurDto>>(
                        $"api/fournisseurs/recherche?terme={enc}");
                return result ?? new List<FournisseurDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FournisseurService.Rechercher] {ex.Message}");
                return new List<FournisseurDto>();
            }
        }

        // ────────────────────────────────────────────────────────
        // AJOUTER (POST)
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Appelle POST /api/fournisseurs avec le DTO de création.
        /// Retourne la réponse contenant l'IdFournisseur généré.
        /// </summary>
        public async Task<FournisseurReponseDto> AjouterAsync(CreerFournisseurDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/fournisseurs", dto);

                if (response.IsSuccessStatusCode)
                {
                    var reponse = await response.Content
                        .ReadFromJsonAsync<FournisseurReponseDto>();
                    return reponse ?? Echec("Réponse vide du serveur.");
                }

                return Echec($"Erreur serveur : {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Echec(ex.Message);
            }
        }

        // ────────────────────────────────────────────────────────
        // MODIFIER (PUT)
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Appelle PUT /api/fournisseurs/{id} avec le DTO de modification.
        /// </summary>
        public async Task<FournisseurReponseDto> ModifierAsync(ModifierFournisseurDto dto)
        {
            try
            {
                var response = await _http
                    .PutAsJsonAsync($"api/fournisseurs/{dto.IdFournisseur}", dto);

                if (response.IsSuccessStatusCode)
                    return new FournisseurReponseDto
                    {
                        Succes  = true,
                        Message = "Fournisseur modifié avec succès."
                    };

                return Echec($"Erreur serveur : {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Echec(ex.Message);
            }
        }

        // ────────────────────────────────────────────────────────
        // SUPPRIMER (DELETE)
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Appelle DELETE /api/fournisseurs/{id}.
        /// </summary>
        public async Task<FournisseurReponseDto> SupprimerAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/fournisseurs/{id}");

                if (response.IsSuccessStatusCode)
                    return new FournisseurReponseDto
                    {
                        Succes  = true,
                        Message = "Fournisseur supprimé avec succès."
                    };

                return Echec($"Erreur serveur : {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Echec(ex.Message);
            }
        }

        // ── Helper privé pour construire une réponse d'échec ────
        private static FournisseurReponseDto Echec(string message) =>
            new() { Succes = false, Message = message };
    }
}
