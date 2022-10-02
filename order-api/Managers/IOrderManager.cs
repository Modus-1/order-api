using MongoDB.Driver;
using order_api.Models;

namespace order_api.Managers;

public interface IOrderManager
{
    public List<Order> Orders { get; set; }
    public Response AddOrder(Order order);
    public bool DeleteOrder(string guid);
    public Order? GetOrder(string guid);
    public List<Order> GetOrderSubset(OrderStatus? status = null, int page = 1);
    public Task SaveOrderToDatabase(Order order);
}