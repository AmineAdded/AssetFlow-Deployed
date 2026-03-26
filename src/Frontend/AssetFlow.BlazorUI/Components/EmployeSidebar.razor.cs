// ============================================================
// AssetFlow.BlazorUI / Components / EmployeSidebar.razor.cs
// Logique du composant sidebar employé réutilisable
// ============================================================

using AssetFlow.BlazorUI.Services;
using Microsoft.AspNetCore.Components;

namespace AssetFlow.BlazorUI.Components
{
    public partial class EmployeSidebar
    {
        [Inject] private EmployeService EmployeService { get; set; } = default!;

        /// <summary>
        /// Page active pour surligner le bon lien nav.
        /// Valeurs acceptées : "equipements" | "incident" | "messagerie"
        /// </summary>
        [Parameter] public string ActivePage { get; set; } = string.Empty;

        private string UserName { get; set; } = "Utilisateur";
        private string UserRole { get; set; } = "Employé";

        protected override async Task OnInitializedAsync()
        {
            UserName = await EmployeService.GetCurrentUserNameAsync();
            UserRole = await EmployeService.GetCurrentUserRoleAsync();
        }

        private string GetInitials()
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2) return parts[0][..2].ToUpper();
            return "??";
        }
    }
}