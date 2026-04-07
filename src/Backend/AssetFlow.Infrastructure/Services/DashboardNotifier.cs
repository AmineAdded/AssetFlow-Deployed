// AssetFlow.Infrastructure/Services/DashboardNotifier.cs
using AssetFlow.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AssetFlow.Infrastructure.Services
{
    public class DashboardNotifier : IDashboardNotifier
    {
        private readonly Func<Task> _notify;
        private readonly Func<Task> _notifyIT;

        public DashboardNotifier(Func<Task> notify, Func<Task> notifyIT)
        {
            _notify   = notify;
            _notifyIT = notifyIT;
        }

        public Task NotifyAsync() => _notify();
        public Task NotifyITAsync() => _notifyIT(); 
    }
}