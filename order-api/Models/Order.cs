namespace order_api.Models
{
    /// <summary>
    /// Represents an order.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Creates an order.
        /// </summary>
        public Order()
        {
            
        }

        /// <summary>
        /// The maximum number of items an order can have.
        /// </summary>
        public const int MAX_ITEMS = 255;

        /// <summary>
        /// The ID of the order.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the total order price.
        /// </summary>
        public decimal TotalPrice { get; set; } = 0;

        /// <summary>
        /// Gets or sets the list of order items.
        /// </summary>
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// The ID of the table.
        /// </summary>
        public int TableId { get; set; } = 0;

        /// <summary>
        /// The session ID of this order.
        /// </summary>
        //public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order status.
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.PLACED;

        /// <summary>
        /// Adds an item to this order.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void AddItem(OrderItem item)
        {
            // Check order item
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Order item cannot be null");

            if (item.Amount < 1)
                throw new ArgumentOutOfRangeException(nameof(item.Amount), "Amount must be greater than 1");

            if (string.IsNullOrEmpty(item.Name))
                throw new ArgumentOutOfRangeException(nameof(item.Name), "Item name cannot be empty.");

            if (item.Id < 0)
                throw new ArgumentOutOfRangeException(nameof(item.Id), "Item ID must be a positive integer");

            // Check if item ID is already present
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            OrderItem prevItem = Items.Find((o) => o.Id == item.Id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (prevItem != null)
                throw new Exception("This item already exists.");

            // Check if too many items have been ordered
            int totalItems = 0;

            foreach (OrderItem oItem in Items)
                totalItems += oItem.Amount;

            // Perform check (include proposed item amount)
            if ((totalItems + item.Amount) > MAX_ITEMS)
                throw new ArgumentOutOfRangeException(nameof(item.Amount), "Maximum number of items in order has been reached.");

            // Add the item
            Items.Add(item);
        }
    }
}
