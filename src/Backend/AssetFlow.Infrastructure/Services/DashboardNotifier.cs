// AssetFlow.Infrastructure/Services/DashboardNotifier.cs
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AssetFlow.Infrastructure.Services
{
    public class DashboardNotifier : IDashboardNotifier
    {
        private readonly Func<Task> _notify;

        public DashboardNotifier(Func<Task> notify) => _notify = notify;

        public Task NotifyAsync() => _notify();
    }
}