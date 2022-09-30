namespace order_api.Models.SchemaObjects
{
    /// <summary>
    /// Schema object for placing orders.
    /// </summary>
    public class PlaceOrderSchema
    {
        /// <summary>
        /// Gets or sets the total order price.
        /// </summary>
        public decimal TotalPrice { get; set; } = 0;

        /// <summary>
        /// The ID of the table.
        /// </summary>
        public int TableId { get; set; } = 0;
    }
}
