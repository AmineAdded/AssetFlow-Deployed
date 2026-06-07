using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Blazored.LocalStorage;

namespace AssetFlow.BlazorUI.Components
{
    public partial class AdminSidebar : ComponentBase, IAsyncDisposable
    {
        [Inject] private IJSRuntime          JS              { get; set; } = default!;
        [Inject] private NavigationManager   Navigation      { get; set; } = default!;
        [Inject] private DemandeAchatService DemandeAchatSvc { get; set; } = default!;
        [Inject] private StockAlertService   StockAlertSvc   { get; set; } = default!;
        [Inject] private AgentChatService    AgentSvc        { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage   { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        [Parameter] public bool ForceOpen { get; set; } = false;

        private bool   _drawerOpen  = false;
        private bool   _espaceOpen  = false;
        private bool   _itOpen      = false;
        private bool   _achatOpen   = false;
        private string _nom         = "Admin";
        private string _role        = "Administrateur";
        private string _initiales   = "AD";

        // ── Compteurs ──
        private int _nombreNonVus    = 0;
        private int _stockAlertCount => StockAlertSvc.AlertCount;

        // ── SignalR ──
        private HubConnection? _hubDashboard;

        protected override async Task OnInitializedAsync()
        {
            // ── Infos utilisateur ──
            try
            {
                var nom  = await JS.InvokeAsync<string?>("eval", "localStorage.getItem('user_name')");
                var role = await JS.InvokeAsync<string?>("eval", "localStorage.getItem('user_role')");
                if (!string.IsNullOrWhiteSpace(nom))  { _nom  = nom.Trim().Trim('"', '\''); }
                if (!string.IsNullOrWhiteSpace(role)) { _role = role.Trim().Trim('"', '\''); }
                var parts = _nom.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _initiales = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                    : _nom[..Math.Min(2, _nom.Length)].ToUpper();

                // ── Auto-ouverture sous-menus ──
                if (Navigation.Uri.Contains("/demandes-achat")
                    || Navigation.Uri.Contains("/achat/")
                    || Navigation.Uri.Contains("/statistiques")
                    || Navigation.Uri.Contains("/agent-chat"))
                { _espaceOpen = true; _achatOpen = true; }
                else if (Navigation.Uri.Contains("/it")
                    || Navigation.Uri.Contains("/dashboard/it"))
                { _espaceOpen = true; _itOpen = true; }
            }
            catch { }

            // ── Compteurs initiaux ──
            try { _nombreNonVus = await DemandeAchatSvc.GetCountNonVusAsync(); } catch { }
            await RefreshStockAlertsAsync();

            // ── Abonnement StockAlertService ──
            StockAlertSvc.OnChanged += OnStockAlertChanged;

            // ── SignalR ──
            await ConnecterDashboardHubAsync();
        }

        protected override void OnParametersSet()
        {
            if (ForceOpen) _drawerOpen = true;
        }

        // ── Alertes de stock ──────────────────────────────────────────────────
        private async Task RefreshStockAlertsAsync()
        {
            try
            {
                var resp = await AgentSvc.GetInitialAlertsAsync();
                if (resp != null)
                    StockAlertSvc.Set(resp.Alertes.Count);
            }
            catch { }
        }

        // ── SignalR ───────────────────────────────────────────────────────────
        private async Task ConnecterDashboardHubAsync()
        {
            var hubUrl = Http.BaseAddress!.ToString().TrimEnd('/') + "/dashboardhub";
            _hubDashboard = new HubConnectionBuilder()
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

            _hubDashboard.Reconnected += async _ =>
            {
                try { await _hubDashboard.InvokeAsync("JoinDashboard"); } catch { }
                await InvokeAsync(async () =>
                {
                    try
                    {
                        _nombreNonVus = await DemandeAchatSvc.GetCountNonVusAsync();
                        await RefreshStockAlertsAsync();
                    }
                    catch { }
                    finally { StateHasChanged(); }
                });
            };

            _hubDashboard.On("DashboardUpdated", async () =>
            {
                await InvokeAsync(async () =>
                {
                    try
                    {
                        _nombreNonVus = await DemandeAchatSvc.GetCountNonVusAsync();
                        await RefreshStockAlertsAsync();
                    }
                    catch { }
                    finally { StateHasChanged(); }
                });
            });

            try
            {
                await _hubDashboard.StartAsync();
                await _hubDashboard.InvokeAsync("JoinDashboard");
            }
            catch { }
        }

        // ── Callbacks ────────────────────────────────────────────────────────
        private void OnStockAlertChanged() => InvokeAsync(StateHasChanged);

        private bool IsActive(string href)
            => Navigation.Uri.Contains(href, StringComparison.OrdinalIgnoreCase);

        private bool IsActiveIT()
            => Navigation.Uri.Contains("/it/demandes-IT", StringComparison.OrdinalIgnoreCase)
            || Navigation.Uri.Contains("/it/offres/",     StringComparison.OrdinalIgnoreCase);

        public async ValueTask DisposeAsync()
        {
            StockAlertSvc.OnChanged -= OnStockAlertChanged;

            if (_hubDashboard is not null)
            {
                try { await _hubDashboard.InvokeAsync("LeaveDashboard"); } catch { }
                await _hubDashboard.DisposeAsync();
            }
        }
    }
}