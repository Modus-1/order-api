using MongoDB.Driver;
using order_api.Config;
using order_api.Models;

namespace order_api.Managers
{
    /// <summary>
    /// A management object for orders.
    /// </summary>
    public class OrderManager : IOrderManager
    {
        /// <summary>
        /// Gets or sets the Mongo database client.
        /// </summary>
        private MongoClient? DbClient { get; set; }

        /// <summary>
        /// Finished order collection.
        /// </summary>
        private IMongoCollection<Order>? FinishedOrderCol { get; set; }

        /// <summary>
        /// Max items per page.
        /// </summary>
        private static int PaginationMaxItems => 10;

        /// <summary>
        /// A list containing all active orders.
        /// </summary>
        public List<Order> Orders { get; set; } = new List<Order>();

        /// <summary>
        /// Constructs a new order manager.
        /// </summary>
        public OrderManager()
        {
            if (DatabaseConfiguration.DATABASE_URI != string.Empty)
            {
                DbClient = new MongoClient(DatabaseConfiguration.DATABASE_URI);
                FinishedOrderCol = DbClient.GetDatabase(DatabaseConfiguration.DB_NAME).GetCollection<Order>(DatabaseConfiguration.COL_NAME_FINISHED);
            }
        }

        /// <summary>
        /// Registers an order.
        /// </summary>
        /// <param name="order">The order to add.</param>
        /// <returns>Response with details if the addition was successful or not.</returns>
        public Response AddOrder(Order order)
        {
            var message = "";
            
            if (order.TableId < 0)
                message += $" {nameof(order.TableId)} must be greater than or equal to 0.";

            if (order.TotalPrice < 0)
                message += $" {nameof(order.TotalPrice)} must be greater than or equal to 0.";
            
            var alreadyExists = GetOrder(order.Id) is not null;
            if (alreadyExists)
                message += $" An {nameof(Order)} with {nameof(order.Id)} {order.Id} already exists.";

            if (string.IsNullOrWhiteSpace(message))
                Orders.Add(order);

            return new Response
            {
                Successful = string.IsNullOrWhiteSpace(message),
                Message = message
            };
        }

        /// <summary>
        /// Deletes the specified order.
        /// </summary>
        /// <param name="guid">The GUID of the order to delete.</param>
        /// <returns>Whether or not the order could be deleted, boolean.</returns>
        public bool DeleteOrder(string guid)
        {
            var order = GetOrder(guid);
            
            return order is not null 
                   && Orders.Remove(order);
        }

        /// <summary>
        /// Gets an order from the specified ID.
        /// </summary>
        /// <param name="id">The ID of the order to get.</param>
        /// <returns>The order.</returns>
        public Order? GetOrder(string id)
        {
            return Orders.Find((o) => o.Id == id);
        }

        /// <summary>
        /// Gets a subset of orders, depending on status and page.
        /// </summary>
        /// <param name="status">The defined status of the order. If null, all statuses are included.</param>
        /// <param name="page">The page to grab.</param>
        /// <returns></returns>
        public List<Order> GetOrderSubset(OrderStatus? status = null, int page = 1)
        {
            if (status is null) 
                return Orders.Skip((page - 1) * PaginationMaxItems).Take(PaginationMaxItems).ToList();

            return Orders
                .Where(o => o.Status == status)
                .Skip((page - 1) * PaginationMaxItems)
                .Take(PaginationMaxItems)
                .ToList();
        }

        /// <summary>
        /// Saves the order in the database.
        /// </summary>
        /// <param name="order">The order to save in the database.</param>
        public async Task SaveOrderToDatabase(Order order)
        {
            if (FinishedOrderCol is not null)
                await FinishedOrderCol.InsertOneAsync(order);
        }
    }
}
