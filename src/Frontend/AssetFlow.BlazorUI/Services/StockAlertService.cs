// src/Frontend/AssetFlow.BlazorUI/Services/StockAlertService.cs
namespace AssetFlow.BlazorUI.Services
{
    /// <summary>
    /// Service singleton partagé qui maintient le compteur d'alertes de stock
    /// en temps réel pour toutes les pages (sidebar, etc.)
    /// </summary>
    public class StockAlertService
    {
        private int _alertCount = 0;

        public int AlertCount => _alertCount;

        public event Action? OnChanged;

        public void Set(int count)
        {
            if (_alertCount != count)
            {
                _alertCount = count;
                OnChanged?.Invoke();
            }
        }

        public void Decrement()
        {
            if (_alertCount > 0)
            {
                _alertCount--;
                OnChanged?.Invoke();
            }
        }
    }
}
