using order_api.Models;

namespace order_api.Managers
{
    public interface IOrderWebSocketManager
    {
        void SendNewOrder(Order order);
        void UpdateExistingOrder(Order order);
    }
}