// AssetFlow.WebAPI/Hubs/DashboardHub.cs
using Microsoft.AspNetCore.SignalR;

namespace AssetFlow.WebAPI.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task JoinDashboard()
            => await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");

        public async Task LeaveDashboard()
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, "dashboard");
    }
}