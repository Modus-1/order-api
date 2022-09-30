using MongoDB.Driver;
using order_api.Config;
using order_api.Models;

namespace order_api
{
    /// <summary>
    /// A management object for orders.
    /// </summary>
    public class OrderManager
    {
        /// <summary>
        /// Gets or sets the Mongo database client.
        /// </summary>
        private MongoClient DBClient { get; set; }

        /// <summary>
        /// Finished order collection.
        /// </summary>
        internal IMongoCollection<Order> FinishedOrderCol { get; set; }

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
                DBClient = new MongoClient(DatabaseConfiguration.DATABASE_URI);
                FinishedOrderCol = DBClient.GetDatabase(DatabaseConfiguration.DB_NAME).GetCollection<Order>(DatabaseConfiguration.COL_NAME_FINISHED);
            }
        }

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

//            if (string.IsNullOrEmpty(order.SessionId))
//                throw new ArgumentNullException(nameof(order.SessionId), "Session ID cannot be null");

            if (order.TotalPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(order.TotalPrice), "Price must be a positive decimal.");

            // Check if order already exists
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Order prevOrder = Orders.Find((o) => o.Id == order.Id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (prevOrder != null)
                throw new ArgumentException("Order already exists.");

            Orders.Add(order);
        }

        /// <summary>
        /// Deletes the specified order.
        /// </summary>
        /// <param name="guid">The GUID of the order to delete.</param>
        public void DeleteOrder(string guid)
        {
            Order? order = Get(guid);

            if (order == null)
                throw new ArgumentException("Item not found", nameof(guid));

            Orders.Remove(order);
        }

        /// <summary>
        /// Gets an order from the specified ID.
        /// </summary>
        /// <param name="id">The ID of the order to get.</param>
        /// <returns>The order.</returns>
        public Order Get(string id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return Orders.Find((o) => o.Id == id);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
