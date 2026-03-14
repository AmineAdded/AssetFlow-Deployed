// // ============================================================
// // AdminLayout.razor.cs
// // ============================================================

// using AssetFlow.BlazorUI.Services;
// using Microsoft.AspNetCore.Components;

// namespace AssetFlow.BlazorUI.Layout
// {
//     public partial class AdminLayout
//     {
//         [Inject] private AuthService AuthService { get; set; } = default!;
//         [Inject] private NavigationManager Navigation { get; set; } = default!;

//         private async Task HandleLogout()
//         {
//             await AuthService.LogoutAsync();
//             Navigation.NavigateTo("/");
//         }
//     }
// }