// ============================================================
// AssetFlow.BlazorUI / Pages / RoleSelection / RoleSelect.razor.cs
// Logique : sélection de rôle + modal admin déclenché par frappe clavier
// ============================================================

using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AssetFlow.BlazorUI.Pages.RoleSelection
{
    public partial class RoleSelect
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private AuthService AuthService { get; set; } = default!;

        // ── Buffer clavier pour détecter "admin" ──
        private string KeyBuffer { get; set; } = string.Empty;
        private DateTime LastKeyTime { get; set; } = DateTime.MinValue;
        private const int KeyTimeoutMs = 1500; // reset buffer si pause > 1.5s

        // ── État modal ──
        private bool ShowAdminModal { get; set; } = false;

        // ── Champs du formulaire admin ──
        private string AdminEmail    { get; set; } = string.Empty;
        private string AdminPassword { get; set; } = string.Empty;
        private bool   ShowAdminPassword { get; set; } = false;

        // ── États UI du modal ──
        private bool   AdminIsLoading    { get; set; } = false;
        private string AdminErrorMessage { get; set; } = string.Empty;
        private bool   AdminEmailError   { get; set; } = false;

        // ────────────────────────────────────────────────────
        // Gestion des touches clavier sur la page principale
        // ────────────────────────────────────────────────────
        private void HandleKeyDown(KeyboardEventArgs e)
        {
            // Ne pas interférer si le modal est déjà ouvert
            if (ShowAdminModal) return;

            // Ignorer les touches non-caractères
            if (e.Key.Length != 1) return;

            var now = DateTime.Now;

            // Reset buffer si trop de temps s'est écoulé
            if ((now - LastKeyTime).TotalMilliseconds > KeyTimeoutMs)
                KeyBuffer = string.Empty;

            LastKeyTime = now;
            KeyBuffer += e.Key.ToLower();
            Console.WriteLine($"KeyBuffer: {KeyBuffer}"); // Debug

            // Ne garder que les 5 derniers caractères
            if (KeyBuffer.Length > 5)
                KeyBuffer = KeyBuffer[^5..];

            // Vérifier si "admin" a été tapé
            if (KeyBuffer.EndsWith("admin"))
            {
                KeyBuffer = string.Empty;
                OpenAdminModal();
            }
        }

        // ────────────────────────────────────────────────────
        // Touche Entrée dans les inputs du modal
        // ────────────────────────────────────────────────────
        private async Task HandleModalKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                await HandleAdminLogin();
            else if (e.Key == "Escape")
                CloseModal();
        }

        // ────────────────────────────────────────────────────
        // Ouvrir / Fermer le modal
        // ────────────────────────────────────────────────────
        private void OpenAdminModal()
        {
            AdminEmail        = string.Empty;
            AdminPassword     = string.Empty;
            AdminErrorMessage = string.Empty;
            AdminEmailError   = false;
            AdminIsLoading    = false;
            ShowAdminPassword = false;
            ShowAdminModal    = true;
        }

        private void CloseModal()
        {
            ShowAdminModal    = false;
            AdminErrorMessage = string.Empty;
            AdminEmailError   = false;
        }

        // ────────────────────────────────────────────────────
        // Toggle visibilité mot de passe
        // ────────────────────────────────────────────────────
        private void ToggleAdminPassword() => ShowAdminPassword = !ShowAdminPassword;

        // ────────────────────────────────────────────────────
        // Connexion admin
        // ────────────────────────────────────────────────────
        private async Task HandleAdminLogin()
        {
            AdminErrorMessage = string.Empty;
            AdminEmailError   = false;

            if (!AdminEmail.Contains("@"))
            {
                AdminEmailError = true;
                return;
            }

            if (string.IsNullOrEmpty(AdminPassword))
            {
                AdminErrorMessage = "Veuillez entrer votre mot de passe.";
                return;
            }

            AdminIsLoading = true;

            var request = new LoginRequest
            {
                Email    = AdminEmail,
                Password = AdminPassword,
                Role     = "Admin"
            };

            var (success, message) = await AuthService.LoginAsync(request);

            AdminIsLoading = false;

            if (success)
            {
                ShowAdminModal = false;
                Navigation.NavigateTo("/admin/projets");
            }
            else
            {
                AdminErrorMessage = message;
            }
        }

        // ────────────────────────────────────────────────────
        // Navigation vers login par rôle
        // ────────────────────────────────────────────────────
        private void SelectRole(string role)
        {
            Navigation.NavigateTo($"/login?role={role}");
        }
    }
}