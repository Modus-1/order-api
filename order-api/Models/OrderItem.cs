namespace order_api.Models
{
    /// <summary>
    /// Represents an order item.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// The Id of this item.
        /// </summary>
        public int Id { get; set; } = 0;

        /// <summary>
        /// The quantity of this item.
        /// </summary>
        public int Amount { get; set; } = 0;

        /// <summary>
        /// The name of this item.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
