// AssetFlow.Application/Interfaces/IDashboardNotifier.cs
namespace AssetFlow.Application.Interfaces
{
    public interface IDashboardNotifier
    {
        Task NotifyAsync();
        Task NotifyITAsync();
    }
}