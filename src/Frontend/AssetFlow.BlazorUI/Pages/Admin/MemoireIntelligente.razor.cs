using AssetFlow.BlazorUI.DTOs;
using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AssetFlow.BlazorUI.Pages.Admin
{
    public partial class MemoireIntelligente : ComponentBase, IAsyncDisposable
    {
        [Inject] private GraphService    GraphSvc { get; set; } = default!;
        [Inject] private IJSRuntime      JS       { get; set; } = default!;

        // ── État ──────────────────────────────────────────────────────────────
        private bool              _sidebarOpen = false;
        private bool              _loading     = true;
        private string            _error       = string.Empty;
        private bool              _darkMode    = true;
        private bool              _intelligence = false;
        private string            _search      = string.Empty;
        private DateTime?         _lastSync;

        private GraphResponseDto? _data;
        private GraphInsightDto?  _selectedInsight;
        private DotNetObjectReference<MemoireIntelligente>? _dotnetRef;

        // ── Computed ──────────────────────────────────────────────────────────
        private List<GraphInsightDto> _filteredInsights =>
            string.IsNullOrWhiteSpace(_search)
                ? (_data?.Insights ?? new())
                : (_data?.Insights ?? new())
                    .Where(i => i.Title.Contains(_search, StringComparison.OrdinalIgnoreCase)
                             || i.Message.Contains(_search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

        // ── Lifecycle ─────────────────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            // Charger préférence thème
            try
            {
                var saved = await JS.InvokeAsync<string?>("eval", "localStorage.getItem('mi_dark_mode')");
                if (saved == "false") _darkMode = false;
            }
            catch { /* Pas de localStorage en SSR */ }

            await LoadData();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _data != null && !_loading)
            {
                await InitGraph();
            }
        }

        // ── Chargement données ────────────────────────────────────────────────
        private async Task LoadData()
        {
            _loading = true;
            _error   = string.Empty;
            StateHasChanged();

            try
            {
                _data     = await GraphSvc.GetGraphAsync();
                _lastSync = DateTime.UtcNow;

                if (_data == null)
                {
                    _error = "Impossible de charger le graphe. Vérifiez la connexion au backend.";
                }
            }
            catch (Exception ex)
            {
                _error = $"Erreur : {ex.Message}";
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }

            // Initialiser le graphe après rendu
            if (_data != null)
            {
                await Task.Delay(50); // Laisser le DOM se mettre à jour
                await InitGraph();
            }
        }

        // ── Graphe JS ─────────────────────────────────────────────────────────
        private async Task InitGraph()
        {
            if (_data == null) return;
            try
            {
                _dotnetRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("GraphEngine.init", "mi-canvas", _dotnetRef);
                await JS.InvokeVoidAsync("GraphEngine.setData",
                    _data.Nodes, _data.Links);
                await JS.InvokeVoidAsync("GraphEngine.setIntelligenceMode", _intelligence);
            }
            catch { /* Canvas pas encore prêt */ }
        }

        // ── Callback depuis JS (clic sur un nœud) ────────────────────────────
        [JSInvokable]
        public async Task OnNodeClicked(string nodeId)
        {
            // Chercher d'abord dans les insights existants
            var existing = _data?.Insights.FirstOrDefault(i => i.EntityId == nodeId);
            if (existing != null)
            {
                _selectedInsight = existing;
                StateHasChanged();
                return;
            }

            // Sinon demander au backend
            var insight = await GraphSvc.GetNodeInsightAsync(nodeId);
            if (insight != null)
            {
                _selectedInsight = insight;
                StateHasChanged();
            }
        }

        // ── Actions ───────────────────────────────────────────────────────────
        private async Task ToggleDark()
        {
            _darkMode = !_darkMode;
            try
            {
                await JS.InvokeVoidAsync("eval",
                    $"localStorage.setItem('mi_dark_mode', '{_darkMode.ToString().ToLower()}')");
            }
            catch { }
        }

        private async Task ToggleIntelligence()
        {
            _intelligence = !_intelligence;
            try
            {
                await JS.InvokeVoidAsync("GraphEngine.setIntelligenceMode", _intelligence);
            }
            catch { }
            StateHasChanged();
        }

        private async Task FocusNode(string nodeId)
        {
            try
            {
                await JS.InvokeVoidAsync("GraphEngine.highlight", nodeId);
            }
            catch { }
        }

        private void SelectInsight(GraphInsightDto insight)
        {
            _selectedInsight = _selectedInsight == insight ? null : insight;
        }

        private void OnSearch(Microsoft.AspNetCore.Components.ChangeEventArgs e)
        {
            _search = e.Value?.ToString() ?? string.Empty;
            StateHasChanged();
        }

        private void ClearSearch()
        {
            _search = string.Empty;
            StateHasChanged();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static string InsightTypeLabel(string type) => type switch
        {
            "warning"        => "Anomalie",
            "correlation"    => "Corrélation",
            "recommendation" => "Recommandation",
            _                => "Information"
        };

        private static string FormatTime(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 1)  return "à l'instant";
            if (diff.TotalHours   < 1)  return $"il y a {(int)diff.TotalMinutes} min";
            if (diff.TotalDays    < 1)  return $"il y a {(int)diff.TotalHours} h";
            return dt.ToString("dd/MM HH:mm");
        }

        // ── Dispose ───────────────────────────────────────────────────────────
        public async ValueTask DisposeAsync()
        {
            try
            {
                await JS.InvokeVoidAsync("GraphEngine.destroy");
            }
            catch { }
            _dotnetRef?.Dispose();
        }
    }
}