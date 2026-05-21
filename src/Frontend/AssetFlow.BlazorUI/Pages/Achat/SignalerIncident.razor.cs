using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using AssetFlow.BlazorUI.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Blazored.LocalStorage;

namespace AssetFlow.BlazorUI.Pages.Achat
{
    public partial class SignalerIncident:IAsyncDisposable
    {
        // ── Paramètre URL optionnel ────────────────────────────
        // Renseigné depuis DetailsEquipement via NaviguerVersSignalement()
        // Vaut 0 si on arrive depuis /employe/incident (sidebar)
        [Parameter] public int AffectationId { get; set; } = 0;
        [Parameter] public int ArticleId { get; set; } = 0;
        [Inject] private IJSRuntime           JS           { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        private List<ArticleAffecteDto> Articles { get; set; } = new();
        private List<MaterielAffecteGroupeDto> Groupes { get; set; } = new();
        private int SelectedArticleId { get; set; } = 0;

        // ── Injections ─────────────────────────────────────────
        [Inject] private IncidentService IncidentService { get; set; } = default!;
        [Inject] private EmployeService EmployeService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;
        // ── Formulaire ──────────────────────────────────────────
        private string TypeIncident { get; set; } = "Panne";
        private string Description { get; set; } = string.Empty;
        private int Urgence { get; set; } = 50;

        // ── États ───────────────────────────────────────────────
        private bool IsLoading { get; set; } = true;
        private bool IsSubmitting { get; set; } = false;
        private string ErrorMessage { get; set; } = string.Empty;

        // ── Infos utilisateur ──────────────────────────────────
        private string UserName { get; set; } = "Utilisateur";
        private bool        _sidebarOpen     = false;

        private void ToggleSidebar() => _sidebarOpen  = !_sidebarOpen;
        private string      _roleUtilisateur = "Service Achat";
        private bool _estAdmin => _roleUtilisateur.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        private bool _roleCharge = false; 
        private HubConnection? _hubConnection;

        // ── Initialisation ─────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            UserName = await EmployeService.GetCurrentUserNameAsync();
            _roleUtilisateur = await EmployeService.GetCurrentUserRoleAsync(); // ← lire le bon champ !
            _roleCharge = true;

            Groupes = await EmployeService.GetMaterielsGroupesAsync();
            Articles = Groupes.SelectMany(g => g.Articles).ToList();

            if (ArticleId > 0 && Articles.Any(a => a.ArticleId == ArticleId))
                SelectedArticleId = ArticleId;

            IsLoading = false;
            await ConnecterSignalR();
        }
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                try { await _hubConnection.InvokeAsync("LeaveDashboard"); } catch { }
                await _hubConnection.DisposeAsync();
            }
        }
        private async Task ConnecterSignalR()
        {
            var hubUrl = Http.BaseAddress!.ToString().TrimEnd('/') + "/dashboardhub";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        try
                        {
                            return await JS.InvokeAsync<string?>("eval",
                                "localStorage.getItem('access_token') || localStorage.getItem('token')");
                        }
                        catch { return null; }
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On("DashboardUpdated", async () =>
            {
                await InvokeAsync(async () =>
                {
                    try
                    {
                        Groupes  = await EmployeService.GetMaterielsGroupesAsync();
                        Articles = Groupes.SelectMany(g => g.Articles).ToList();

                        // Si l'article sélectionné n'existe plus, réinitialiser
                        if (SelectedArticleId > 0 && !Articles.Any(a => a.ArticleId == SelectedArticleId))
                            SelectedArticleId = 0;
                    }
                    catch { /* silencieux */ }
                    finally { StateHasChanged(); }
                });
            });

            try
            {
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("JoinDashboard");
            }
            catch { /* reste statique si SignalR non dispo */ }
        }

        // ── Sélection du type d'incident ───────────────────────
        private void SelectType(string type)
        {
            TypeIncident = type;
            StateHasChanged();
        }

        // ── Slider urgence ─────────────────────────────────────
        private void OnUrgencyChange(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int value))
            {
                Urgence = value;
                StateHasChanged();
            }
        }

        // ── Soumission ─────────────────────────────────────────
        private async Task SoumettreIncident()
        {
            ErrorMessage = string.Empty;

            // Validation sur SelectedArticleId
            if (SelectedArticleId <= 0)
            {
                ErrorMessage = "Veuillez sélectionner un article.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Veuillez décrire le problème.";
                return;
            }

            try
            {
                IsSubmitting = true;

                var article = Articles.FirstOrDefault(a => a.ArticleId == SelectedArticleId);
                if (article == null) { ErrorMessage = "Article introuvable."; return; }

                var result = await IncidentService.SignalerIncidentAsync(new SignalerIncidentRequestDto
                {
                    AffectationId = article.AffectationId,  // ← tiré de l'article, pas d'une variable séparée
                    ArticleId = article.ArticleId,
                    TypeIncident = TypeIncident,
                    Urgence = Urgence,
                    Description = Description
                });

                if (result.Success)
                    Navigation.NavigateTo($"/achat/incident/success?numero={result.NumeroIncident}");
                else
                    ErrorMessage = result.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erreur : {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }
        private string GetUrgencyLabel()
        {
            if (Urgence <= 33) return "FAIBLE";
            if (Urgence <= 66) return "MOYEN";
            return "CRITIQUE";
        }

        private string GetUrgencyClass()
        {
            if (Urgence <= 33) return "urgency-low";
            if (Urgence <= 66) return "urgency-medium";
            return "urgency-critical";
        }

        private string GetUserInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2)
                return parts[0][..2].ToUpper();
            return "??";
        }
        private string GetDesignation(int affectationId)
        {
            var groupe = Groupes.FirstOrDefault(g =>
                g.Articles.Any(a => a.AffectationId == affectationId));
            return groupe?.Designation ?? "Équipement";
        }
    }
}