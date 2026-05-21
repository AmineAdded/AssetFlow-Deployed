using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using AssetFlow.BlazorUI.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Blazored.LocalStorage;

namespace AssetFlow.BlazorUI.Pages.Employe
{
    public partial class SignalerIncident:IAsyncDisposable
    {
        [Inject] private IJSRuntime           JS           { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;
        [Parameter] public int AffectationId { get; set; } = 0;
        [Parameter] public int ArticleId     { get; set; } = 0;

        private List<ArticleAffecteDto>        Articles { get; set; } = new();
        private List<MaterielAffecteGroupeDto> Groupes  { get; set; } = new();
        private int SelectedArticleId { get; set; } = 0;

        [Inject] private IncidentService   IncidentService { get; set; } = default!;
        [Inject] private EmployeService    EmployeService  { get; set; } = default!;
        [Inject] private NavigationManager Navigation      { get; set; } = default!;

        private string TypeIncident { get; set; } = "Panne";
        private string Description  { get; set; } = string.Empty;
        private int    Urgence      { get; set; } = 50;

        private bool   IsLoading    { get; set; } = true;
        private bool   IsSubmitting { get; set; } = false;
        private string ErrorMessage { get; set; } = string.Empty;
        private bool _menuOpen = false;
        private string CurrentDesignation =>
        SelectedArticleId > 0
           ? GetDesignation(Articles.FirstOrDefault(a => a.ArticleId == SelectedArticleId)?.AffectationId ?? 0)
           : string.Empty;
        private HubConnection? _hubConnection;
        protected override async Task OnInitializedAsync()
        {
            Groupes  = await EmployeService.GetMaterielsGroupesAsync();
            Articles = Groupes.SelectMany(g => g.Articles).ToList();

            if (ArticleId > 0 && Articles.Any(a => a.ArticleId == ArticleId))
                SelectedArticleId = ArticleId;

            IsLoading = false;
            await ConnecterSignalR();
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
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
                {
                    try { await _hubConnection.InvokeAsync("LeaveDashboard"); } catch { }
                    await _hubConnection.DisposeAsync();
                }
        }

        private void SelectType(string type) { TypeIncident = type; StateHasChanged(); }

        private void OnUrgencyChange(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int value)) { Urgence = value; StateHasChanged(); }
        }

        private async Task SoumettreIncident()
        {
            ErrorMessage = string.Empty;

            if (SelectedArticleId <= 0) { ErrorMessage = "Veuillez sélectionner un article."; return; }
            if (string.IsNullOrWhiteSpace(Description)) { ErrorMessage = "Veuillez décrire le problème."; return; }

            try
            {
                IsSubmitting = true;
                var article = Articles.FirstOrDefault(a => a.ArticleId == SelectedArticleId);
                if (article == null) { ErrorMessage = "Article introuvable."; return; }

                var result = await IncidentService.SignalerIncidentAsync(new SignalerIncidentRequestDto
                {
                    AffectationId = article.AffectationId,
                    ArticleId     = article.ArticleId,
                    TypeIncident  = TypeIncident,
                    Urgence       = Urgence,
                    Description   = Description
                });

                if (result.Success)
                    Navigation.NavigateTo($"/employe/incident/success?numero={result.NumeroIncident}");
                else
                    ErrorMessage = result.Message;
            }
            catch (Exception ex) { ErrorMessage = $"Erreur : {ex.Message}"; }
            finally               { IsSubmitting = false; }
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

        private string GetDesignation(int affectationId)
        {
            var groupe = Groupes.FirstOrDefault(g => g.Articles.Any(a => a.AffectationId == affectationId));
            return groupe?.Designation ?? "Équipement";
        }
    }
}