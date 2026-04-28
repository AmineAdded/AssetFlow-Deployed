// src/Frontend/AssetFlow.BlazorUI/Pages/AgentChat.razor.cs
using AssetFlow.BlazorUI.DTOs;
using AssetFlow.BlazorUI.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AssetFlow.BlazorUI.Pages.Achat
{
    public partial class AgentChat : ComponentBase
    {
        [Inject] private AgentChatService  AgentSvc     { get; set; } = default!;
        [Inject] private IJSRuntime        JS           { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;

        // ── State ──────────────────────────────────────────────────
        private class ChatMessage
        {
            public bool          IsUser          { get; set; }
            public string        Content         { get; set; } = string.Empty;
            public string?       AgentBadge      { get; set; }
            public AgentAction?  Action          { get; set; }
            public bool          ActionProcessed { get; set; }
            public DateTime      Timestamp       { get; set; } = DateTime.Now;
        }

        private List<ChatMessage>       _messages        = new();
        private List<AlerteStock>       _alertes         = new();
        private List<AgentChatHistory>  _history         = new();

        private string  _inputText    = string.Empty;
        private bool    _isLoading    = false;
        private bool    _isApproving  = false;
        private bool    _showAlertes  = true;
        private bool    _sidebarOpen  = false;
        private string  _initiales    = "U";
        private string? _username     = "Utilisateur";
        private string? _role         = "EquipeAchat";

        private readonly List<string> _suggestions = new()
        {
            "📦 Liste mes matériels en alerte",
            "📊 Donne-moi les statistiques de stock",
            "🔍 Recherche des fournisseurs de PC",
            "➕ Ajoute un nouveau matériel laptop",
            "📋 Montre mes dernières commandes",
            "⚠️ Quels incidents sont en attente ?"
        };

        // ── Init ───────────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            // Charger infos utilisateur
            try
            {
                var nom  = await JS.InvokeAsync<string?>("eval", "localStorage.getItem('user_name')");
                var role = await JS.InvokeAsync<string?>("eval", "localStorage.getItem('user_role')");
                if (!string.IsNullOrWhiteSpace(nom))  _username = Clean(nom);
                if (!string.IsNullOrWhiteSpace(role)) _role     = Clean(role);

                var parts = (_username ?? "U").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _initiales = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                    : (_username ?? "U")[..Math.Min(2, (_username ?? "U").Length)].ToUpper();
            }
            catch { }

            // Charger alertes initiales
            await LoadInitialAlerts();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await ScrollToBottom();
        }

        private async Task LoadInitialAlerts()
        {
            var resp = await AgentSvc.GetInitialAlertsAsync();
            if (resp == null) return;

            _alertes     = resp.Alertes;
            _showAlertes = _alertes.Count > 0;

            if (!string.IsNullOrEmpty(resp.Message))
            {
                _messages.Add(new ChatMessage
                {
                    IsUser     = false,
                    Content    = resp.Message,
                    AgentBadge = "db",
                    Timestamp  = DateTime.Now
                });
            }
        }

        // ── Envoi message ──────────────────────────────────────────
        private async Task SendMessage()
        {
            var text = _inputText.Trim();
            if (string.IsNullOrEmpty(text) || _isLoading) return;

            // Message utilisateur
            _messages.Add(new ChatMessage { IsUser = true, Content = text });
            _history.Add(new AgentChatHistory { Role = "user", Content = text });
            _inputText = string.Empty;
            _isLoading = true;
            StateHasChanged();
            await ScrollToBottom();

            // Appel API
            var request = new AgentChatRequest
            {
                Message = text,
                History = _history.TakeLast(10).ToList()
            };

            var resp = await AgentSvc.ChatAsync(request);
            _isLoading = false;

            if (resp != null)
            {
                var botMsg = new ChatMessage
                {
                    IsUser     = false,
                    Content    = resp.Message,
                    AgentBadge = resp.AgentUsed,
                    Action     = resp.Action,
                    Timestamp  = DateTime.Now
                };
                _messages.Add(botMsg);
                _history.Add(new AgentChatHistory { Role = "assistant", Content = resp.Message });

                // Garder historique limité
                if (_history.Count > 20) _history = _history.TakeLast(20).ToList();
            }
            else
            {
                _messages.Add(new ChatMessage
                {
                    IsUser    = false,
                    Content   = "❌ Une erreur s'est produite. Veuillez réessayer.",
                    Timestamp = DateTime.Now
                });
            }

            StateHasChanged();
            await ScrollToBottom();
        }

        private async Task SendSuggestion(string text)
        {
            _inputText = text;
            await SendMessage();
        }

        // ── Approbation d'action ───────────────────────────────────
        private async Task ApproveAction(ChatMessage msg, bool approved)
        {
            if (msg.Action == null) return;
            _isApproving = true;

            var request = new AgentApprovalRequest
            {
                ActionType      = msg.Action.Type,
                Approved        = approved,
                Utilisateur     = _username ?? "Agent IA",
                MaterielProposal = msg.Action.MaterielProposal,
                CommandeProposal = msg.Action.CommandeProposal,
                ArticleProposal  = msg.Action.ArticleProposal
            };

            var resp = await AgentSvc.ApproveAsync(request, _username ?? "Utilisateur");
            _isApproving     = false;
            msg.ActionProcessed = true;

            var resultMsg = resp?.Message ?? (approved ? "✅ Action effectuée." : "❌ Action annulée.");
            _messages.Add(new ChatMessage
            {
                IsUser    = false,
                Content   = resultMsg,
                AgentBadge = resp?.Succes == true ? "action_success" : "action_error",
                Timestamp = DateTime.Now
            });

            // Recharger les alertes si succès
            if (resp?.Succes == true && msg.Action.Type == "add_materiel")
                await LoadInitialAlerts();

            StateHasChanged();
            await ScrollToBottom();
        }

        // ── Ouvrir proposition depuis alerte ───────────────────────
        private void OpenAlertProposal(AlerteStock alerte)
        {
            if (alerte.Proposition == null) return;

            // Ajouter un message avec le formulaire pré-rempli
            _messages.Add(new ChatMessage
            {
                IsUser     = false,
                Content    = $"📦 Proposition de réapprovisionnement pour **{alerte.Designation}** (stock: {alerte.QuantiteStock}/{alerte.QuantiteMin}). Veuillez vérifier et approuver :",
                AgentBadge = "action",
                Action     = new AgentAction
                {
                    Type             = "add_materiel",
                    Label            = "Réapprovisionnement",
                    MaterielProposal = alerte.Proposition
                },
                Timestamp = DateTime.Now
            });
            _showAlertes = false;
            StateHasChanged();
        }

        // ── Keyboard ───────────────────────────────────────────────
        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
                await SendMessage();
        }

        // ── Clear ──────────────────────────────────────────────────
        private async Task ClearChat()
        {
            _messages.Clear();
            _history.Clear();
            _showAlertes = _alertes.Count > 0;
            await LoadInitialAlerts();
        }

        // ── Scroll ─────────────────────────────────────────────────
        private async Task ScrollToBottom()
        {
            try
            {
                await JS.InvokeVoidAsync("eval",
                    "setTimeout(()=>{const c=document.getElementById('ai-messages-container');if(c)c.scrollTop=c.scrollHeight;},50)");
            }
            catch { }
        }

        // ── Helpers UI ─────────────────────────────────────────────
        private bool _estAdmin => _role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

        private static string GetAgentBadgeClass(string badge) => badge switch
        {
            "web"            => "badge-web",
            "db"             => "badge-db",
            "action"         => "badge-action",
            "action_success" => "badge-success",
            "action_error"   => "badge-error",
            _                => "badge-db"
        };

        private static string GetAgentBadgeIcon(string badge) => badge switch
        {
            "web"            => "🌐",
            "db"             => "🗄️",
            "action"         => "⚡",
            "action_success" => "✅",
            "action_error"   => "❌",
            _                => "🤖"
        };

        private static string GetAgentBadgeLabel(string badge) => badge switch
        {
            "web"            => "Recherche Web",
            "db"             => "Base de données",
            "action"         => "Action proposée",
            "action_success" => "Succès",
            "action_error"   => "Erreur",
            _                => "Agent IA"
        };

        private static string FormatMessage(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // Bold **text**
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            // Italic *text*
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*(.+?)\*", "<em>$1</em>");
            // Code `text`
            text = System.Text.RegularExpressions.Regex.Replace(text, @"`(.+?)`", "<code>$1</code>");
            // Line breaks
            text = text.Replace("\n", "<br/>");

            return text;
        }

        private static string Clean(string v)
        {
            v = v.Trim();
            if (v.Length >= 2 &&
                ((v.StartsWith('"') && v.EndsWith('"')) ||
                 (v.StartsWith('\'') && v.EndsWith('\''))))
                v = v[1..^1].Trim();
            return v;
        }
    }
}
