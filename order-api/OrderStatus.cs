namespace order_api
{
    /// <summary>
    /// An enum representing the status of an order.
    /// </summary>
    public enum OrderStatus : uint
    {
        PLACED = 0,
        PROCESSING = 1,
        READY = 2,
        DONE = 3
    }
}
