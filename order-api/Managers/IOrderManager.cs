using MongoDB.Driver;
using order_api.Models;

namespace order_api.Managers;

public interface IOrderManager
{
    public List<Order> Orders { get; set; }
    public Response AddOrder(Order order);
    public bool DeleteOrder(string guid);
    public Response<Order> GetOrder(string guid);
    public Response<List<Order>> GetOrderSubset(OrderStatus? status = null, int page = 1);
    public Response<List<Order>> GetAllActiveOrders();
    public Response<Order> UpdateOrderDetails(string id, Order newDetails);
    public Response<Order> AddItemsToOrder(string id, OrderItem[] itemsToAdd);
    public Response<OrderItem> GetItemFromOrder(string orderId, string itemId);
    public Response<Order> DeleteItemFromOrder(string orderId, string itemId);
    public Task SaveOrderToDatabase(Order order);
}