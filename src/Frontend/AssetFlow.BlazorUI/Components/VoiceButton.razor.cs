using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AssetFlow.BlazorUI.Components
{
    public partial class VoiceButton : ComponentBase, IAsyncDisposable
    {
        [Inject] private VoiceCommandService VoiceSvc    { get; set; } = default!;
        [Inject] private VoiceNluService     NluSvc      { get; set; } = default!;
        [Inject] private NavigationManager   Nav         { get; set; } = default!;
        [Inject] private IJSRuntime          JS          { get; set; } = default!;

        private bool   _listening   = false;
        private bool   _processing  = false; // pendant l'appel API
        private string _transcript  = string.Empty;
        private string _feedback    = string.Empty;
        private bool   _isError     = false;
        private DotNetObjectReference<VoiceButton>? _dotNetRef;

        protected override void OnInitialized()
        {
            VoiceSvc.OnListeningChanged += OnListeningChanged;
            VoiceSvc.OnTranscript       += OnTranscript;
            VoiceSvc.OnCommand          += HandleNavigation;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            try
            {
                var role = await JS.InvokeAsync<string?>("eval",
                    "localStorage.getItem('user_role')");
                if (!string.IsNullOrWhiteSpace(role))
                    VoiceSvc.SetRole(role);
            }
            catch { }

            _dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("VoiceAssistant.init", _dotNetRef);
        }

        private async Task ToggleListen()
        {
            if (_listening || _processing)
            {
                // Arrêter l'enregistrement → déclenche OnAudioReady
                await JS.InvokeVoidAsync("VoiceAssistant.stop");
                return;
            }

            // Rafraîchir le rôle
            try
            {
                var role = await JS.InvokeAsync<string?>("eval",
                    "localStorage.getItem('user_role')");
                if (!string.IsNullOrWhiteSpace(role))
                    VoiceSvc.SetRole(role);
            }
            catch { }

            _transcript = string.Empty;
            _feedback   = string.Empty;
            _listening  = true;
            VoiceSvc.SetListening(true);
            await JS.InvokeVoidAsync("VoiceAssistant.start");
            StateHasChanged();
        }

        /// <summary>
        /// Appelé par JS quand l'audio est prêt (après stop())
        /// </summary>
        [JSInvokable("OnAudioReady")]
        public async Task OnAudioReady(string audioBase64, string mimeType)
        {
            _listening  = false;
            _processing = true;
            VoiceSvc.SetListening(false);
            StateHasChanged();

            try
            {
                var response = await NluSvc.ProcessAsync(
                    audioBase64, mimeType, VoiceSvc.CurrentRole);

                if (response == null || !string.IsNullOrEmpty(response.Error))
                {
                    await ShowFeedback(
                        response?.Error ?? "Erreur de traitement.", true);
                    return;
                }

                if (string.IsNullOrWhiteSpace(response.Transcript))
                {
                    await ShowFeedback("Aucune parole détectée.", true);
                    return;
                }

                // Afficher la transcription
                _transcript = response.Transcript;
                StateHasChanged();

                // Dispatcher la commande
                await VoiceSvc.DispatchResponse(response);

                // Effacer le transcript après 3s
                await Task.Delay(3000);
                _transcript = string.Empty;
            }
            catch (Exception ex)
            {
                await ShowFeedback($"Erreur : {ex.Message}", true);
            }
            finally
            {
                _processing = false;
                StateHasChanged();
            }
        }

        [JSInvokable("OnError")]
        public async Task OnError(string error)
        {
            _listening  = false;
            _processing = false;
            VoiceSvc.SetListening(false);
            await ShowFeedback("Erreur micro : " + error, true);
            StateHasChanged();
        }

        private Task HandleNavigation(VoiceCommand cmd)
        {
            if (cmd.Type != VoiceCommandType.Navigation || cmd.NavigateTo == null)
                return Task.CompletedTask;

            return InvokeAsync(() =>
            {
                Nav.NavigateTo(cmd.NavigateTo);
                StateHasChanged();
            });
        }

        private void OnListeningChanged(bool v)
        {
            _listening = v;
            InvokeAsync(StateHasChanged);
        }

        private void OnTranscript(string t)
        {
            _transcript = t;
            InvokeAsync(StateHasChanged);
        }

        private async Task ShowFeedback(string msg, bool isError)
        {
            _feedback = msg; _isError = isError;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(3500);
            _feedback = string.Empty;
            await InvokeAsync(StateHasChanged);
        }

        public async ValueTask DisposeAsync()
        {
            VoiceSvc.OnListeningChanged -= OnListeningChanged;
            VoiceSvc.OnTranscript       -= OnTranscript;
            VoiceSvc.OnCommand          -= HandleNavigation;
            try { await JS.InvokeVoidAsync("VoiceAssistant.stop"); } catch { }
            _dotNetRef?.Dispose();
        }
    }
}