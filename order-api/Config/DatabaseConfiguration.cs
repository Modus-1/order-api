namespace order_api.Config
{
    /// <summary>
    /// Configuration object for database.
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// The MongoDB Database URI.
        /// </summary>
        public const string DATABASE_URI = ""; //"mongodb://localhost:27017";

        /// <summary>
        /// The name of the database to store finished orders.
        /// </summary>
        public const string DB_NAME = "ModusOrders";

        /// <summary>
        /// The name of the collection for finished orders.
        /// </summary>
        public const string COL_NAME_FINISHED = "finished";
    }
}
