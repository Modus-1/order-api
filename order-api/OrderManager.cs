using order_api.Models;

namespace order_api
{
    /// <summary>
    /// A management object for orders.
    /// </summary>
    public class OrderManager
    {
        /// <summary>
        /// A list containing all active orders.
        /// </summary>
        public List<Order> Orders { get; private set; } = new List<Order>();

        /// <summary>
        /// Registers an order.
        /// </summary>
        /// <param name="order">The order to add.</param>
        public void AddOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order cannot be null");

            if (order.TableId < 0)
                throw new ArgumentOutOfRangeException(nameof(order.TableId), "Table ID must be a positive integer.");

            if (string.IsNullOrEmpty(order.SessionId))
                throw new ArgumentNullException(nameof(order.SessionId), "Session ID cannot be null");

            if (order.TotalPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(order.TotalPrice), "Price must be a positive integer.");

            // Check if order already exists
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Order prevOrder = Orders.Find((o) => o.Id == order.Id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (prevOrder != null)
                throw new ArgumentException("Order already exists.");

            Orders.Add(order);
        }

        /// <summary>
        /// Gets an order from the specified ID.
        /// </summary>
        /// <param name="id">The ID of the order to get.</param>
        /// <returns>The order.</returns>
        public Order Get(ulong id)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Order o = Orders.Find((o) => o.Id == id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (o == null)
                throw new Exception("Order does not exist.");

            return o;
        }
    }
}
