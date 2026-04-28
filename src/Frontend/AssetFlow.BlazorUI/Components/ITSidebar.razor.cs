using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using AssetFlow.BlazorUI.Services;
using AssetFlow.BlazorUI.DTOs;
using System.Net.Http.Json;

namespace AssetFlow.BlazorUI.Components
{
    public partial class ITSidebar : ComponentBase, IAsyncDisposable
    {
        [Inject] private ILocalStorageService    LocalStorage    { get; set; } = default!;
        [Inject] private UnreadMessagesService   UnreadSvc       { get; set; } = default!;
        [Inject] private MessagerieService       MsgSvc          { get; set; } = default!;
        [Inject] private HttpClient              Http            { get; set; } = default!;

        [Parameter] public string ActivePage { get; set; } = string.Empty;
        [Parameter] public bool   ForceOpen  { get; set; } = false;
        [Parameter] public EventCallback OnClose { get; set; }

        private string UserName    { get; set; } = "IT";
        private bool   _drawerOpen = false;
        private int    _currentUserId = 0;

        private HubConnection? _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            UserName      = await LocalStorage.GetItemAsync<string>("user_name") ?? "IT";
            _currentUserId = await LocalStorage.GetItemAsync<int>("user_id");

            // Charger le compte initial depuis l'API
            await RefreshUnreadCountAsync();

            // S'abonner aux changements pour re-rendre le badge
            UnreadSvc.OnChanged += OnUnreadChanged;

            // Connecter SignalR pour les mises à jour temps réel
            await ConnecterSignalR();
        }

        protected override void OnParametersSet()
        {
            if (ForceOpen && !_drawerOpen)
                _drawerOpen = true;
        }

        private async Task RefreshUnreadCountAsync()
        {
            try
            {
                var summaries = await MsgSvc.GetConversationsAsync(_currentUserId);
                var total = summaries.Sum(s => s.UnreadCount);
                UnreadSvc.Set(total);
            }
            catch { }
        }

        private async Task ConnecterSignalR()
        {
            try
            {
                var token  = await LocalStorage.GetItemAsync<string>("access_token") ?? "";
                var hubUrl = Http.BaseAddress!.ToString().TrimEnd('/') + "/chathub";

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, opts =>
                        opts.AccessTokenProvider = () => Task.FromResult<string?>(token))
                    .WithAutomaticReconnect()
                    .Build();

                // Nouveau message reçu → incrémenter si c'est pour moi et pas la page messagerie
                _hubConnection.On<ChatMessageDto>("ReceiveMessage", async msg =>
                {
                    await InvokeAsync(async () =>
                    {
                        // Si le message est destiné à l'utilisateur courant (pas envoyé par lui)
                        if (msg.ReceiverId == _currentUserId && msg.SenderId != _currentUserId)
                        {
                            // Recalculer depuis l'API pour être exact
                            await RefreshUnreadCountAsync();
                        }
                    });
                });

                // Quand des messages sont lus (par l'autre) → pas besoin de changer notre compteur
                // Quand ON lit des messages → DecreaseUnread est appelé depuis la page Messagerie

                _hubConnection.Reconnected += async _ =>
                {
                    await InvokeAsync(async () =>
                    {
                        try
                        {
                            await _hubConnection.SendAsync("UserConnected", _currentUserId);
                            await RefreshUnreadCountAsync();
                        }
                        catch { }
                    });
                };

                await _hubConnection.StartAsync();
                await _hubConnection.SendAsync("UserConnected", _currentUserId);
            }
            catch { }
        }

        // Callback abonné à UnreadSvc.OnChanged → re-render le badge
        private void OnUnreadChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        private void CloseDrawer() => _drawerOpen = false;

        private string GetInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2) return parts[0][..2].ToUpper();
            return "IT";
        }

        public async ValueTask DisposeAsync()
        {
            UnreadSvc.OnChanged -= OnUnreadChanged;

            if (_hubConnection is not null)
            {
                try { await _hubConnection.SendAsync("UserDisconnected", _currentUserId); } catch { }
                await _hubConnection.DisposeAsync();
            }
        }
    }
}