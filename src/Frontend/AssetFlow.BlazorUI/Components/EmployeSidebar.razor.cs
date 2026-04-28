using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Blazored.LocalStorage;
using AssetFlow.BlazorUI.DTOs;

namespace AssetFlow.BlazorUI.Components
{
    public partial class EmployeSidebar : ComponentBase, IAsyncDisposable
    {
        [Inject] private EmployeService          EmployeService { get; set; } = default!;
        [Inject] private UnreadMessagesService   UnreadSvc      { get; set; } = default!;
        [Inject] private MessagerieService       MsgSvc         { get; set; } = default!;
        [Inject] private ILocalStorageService    LocalStorage   { get; set; } = default!;
        [Inject] private HttpClient              Http           { get; set; } = default!;

        [Parameter] public string ActivePage { get; set; } = string.Empty;
        [Parameter] public bool   ForceOpen  { get; set; } = false;

        private bool   _drawerOpen    = false;
        private int    _currentUserId = 0;
        private string UserName       { get; set; } = "Utilisateur";
        private string UserRole       { get; set; } = "Employé";

        private HubConnection? _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            UserName      = await EmployeService.GetCurrentUserNameAsync();
            UserRole      = await EmployeService.GetCurrentUserRoleAsync();
            _currentUserId = await LocalStorage.GetItemAsync<int>("user_id");

            // Charger le compteur initial
            await RefreshUnreadCountAsync();

            // S'abonner aux changements
            UnreadSvc.OnChanged += OnUnreadChanged;

            // Connexion SignalR pour mises à jour temps réel
            await ConnecterSignalR();
        }

        protected override void OnParametersSet()
        {
            if (ForceOpen) _drawerOpen = true;
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

                // Nouveau message → recalculer le compteur
                _hubConnection.On<ChatMessageDto>("ReceiveMessage", async msg =>
                {
                    await InvokeAsync(async () =>
                    {
                        if (msg.ReceiverId == _currentUserId && msg.SenderId != _currentUserId)
                        {
                            await RefreshUnreadCountAsync();
                        }
                    });
                });

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

        private void OnUnreadChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        private string GetInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2) return parts[0][..2].ToUpper();
            return "??";
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