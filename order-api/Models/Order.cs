using System;

namespace order_api.Models
{
    /// <summary>
    /// Represents an order.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Global created order count.
        /// </summary>
        private static int createdOrders = 0;

        /// <summary>
        /// The rollover count - the point where the order numbers roll over back to 0.
        /// </summary>
        public const int ORDER_NUM_MAX_ROLLOVER = 1000;

        /// <summary>
        /// Creates an order.
        /// </summary>
        public Order()
        {
            // Rollover number
            if (createdOrders > ORDER_NUM_MAX_ROLLOVER)
                createdOrders = 0;

            Number = createdOrders++;
        }

        /// <summary>
        /// The ID of the order.
        /// </summary>
        public string Id { get; init; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the total order price.
        /// </summary>
        public decimal TotalPrice { get; set; } = 0;

        /// <summary>
        /// Gets or sets the list of order items.
        /// </summary>
        public List<OrderItem> Items { get; init; } = new();

        /// <summary>
        /// The ID of the table.
        /// </summary>
        public int TableId { get; set; } = 0;

        /// <summary>
        /// Specifies when the order was created.
        /// </summary>
        public DateTime CreationTime { get; init; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the order status.
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.PLACED;

        /// <summary>
        /// Any special notes the customer has added to their order.
        /// </summary>
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// The order number.
        /// </summary>
        public int Number { get; set; } = 0;
    }
}
