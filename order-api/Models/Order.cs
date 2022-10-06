using System;

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
        public DateTime CreationTime => DateTime.Now;

        /// <summary>
        /// Gets or sets the order status.
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.PLACED;
    }
}
