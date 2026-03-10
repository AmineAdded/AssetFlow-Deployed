// ============================================================
// AssetFlow.BlazorUI / Pages / IT / OffresDemandeIT.razor.cs
// ============================================================
using AssetFlow.Application.DTOs;
using AssetFlow.BlazorUI.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace AssetFlow.BlazorUI.Pages.IT
{
    public partial class OffresDemandeIT
    {
        // ── Injections ───────────────────────────────────────────
        [Parameter]
        public int DemandeId { get; set; }

        [Inject] private OffreITClientService    OffreService   { get; set; } = default!;
        [Inject] private DemandeAchatITClientService DemandeService { get; set; } = default!;
        [Inject] private NavigationManager        Navigation     { get; set; } = default!;
        [Inject] private ILocalStorageService     LocalStorage   { get; set; } = default!;

        // ── État UI ──────────────────────────────────────────────
        private bool   IsLoading    { get; set; } = true;
        private bool   _menuOpen    = false;
        private string UserName     { get; set; } = "IT";
        private string ErrorMessage { get; set; } = string.Empty;

        // ── Données ──────────────────────────────────────────────
        private DemandeAchatITDto?       Demande            { get; set; }
        private List<OffreFournisseurDto> Offres             { get; set; } = new();
        private int                       SelectedOffreIndex { get; set; } = 0;

        private OffreFournisseurDto? SelectedOffre =>
            Offres.Count > 0 && SelectedOffreIndex < Offres.Count
                ? Offres[SelectedOffreIndex]
                : null;

        // ── Init ─────────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            UserName = await LocalStorage.GetItemAsync<string>("user_name") ?? "IT";
            await LoadDataAsync();
        }

        // ── Chargement ───────────────────────────────────────────
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            StateHasChanged();
            try
            {
                var demandesTask = DemandeService.GetDemandesAsync();
                var offresTask   = OffreService.GetOffresParDemandeAsync(DemandeId);

                await Task.WhenAll(demandesTask, offresTask);

                var demandes = await demandesTask;
                Demande = demandes.FirstOrDefault(d => d.IdDemande == DemandeId);

                Offres = await offresTask;

                // Marquer la meilleure offre (prix le plus bas)
                if (Offres.Any())
                {
                    var minPrix = Offres.Min(o => o.PrixUnitaireHT);
                    foreach (var o in Offres)
                        o.EstMeilleurOffre = o.PrixUnitaireHT == minPrix;
                }

                if (Demande is null)
                    ErrorMessage = "Demande introuvable.";
            }
            catch
            {
                ErrorMessage = "Impossible de charger les données. Veuillez réessayer.";
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        // ── Sélection d'offre ────────────────────────────────────
        private void SelectOffre(int index)
        {
            SelectedOffreIndex = index;
        }

        // ── Sauvegarde notes ─────────────────────────────────────
        private async Task SaveNotes(OffreFournisseurDto offre)
        {
            try
            {
                await OffreService.UpdateNotesAsync(offre.IdOffre, offre.Notes);
            }
            catch
            {
                ErrorMessage = "Erreur lors de la sauvegarde. Veuillez réessayer.";
            }
        }

        // ── Helpers ──────────────────────────────────────────────
        private string GetInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2) return parts[0][..2].ToUpper();
            return "IT";
        }

        private static string GetStatutLabel(string statut) => statut switch
        {
            "en_attente" => "En attente",
            "traite"     => "Traité",
            "approuve"   => "Approuvé",
            "refuse"     => "Refusé",
            _            => statut
        };
    }
}
