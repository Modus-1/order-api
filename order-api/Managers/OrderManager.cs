﻿using MongoDB.Driver;
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
            
            var alreadyExists = GetOrder(order.Id).Successful;
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
            var response = GetOrder(guid);
            
            return response.Successful 
                   && response.Data is not null
                   && Orders.Remove(response.Data);
        }

        /// <summary>
        /// Gets an order from the specified ID.
        /// </summary>
        /// <param name="id">The ID of the order to get.</param>
        /// <returns>The order.</returns>
        public Response<Order> GetOrder(string id)
        {
            var foundOrder = Orders.Find(order => order.Id == id);

            return new Response<Order>
            {
                Data = foundOrder,
                Successful = foundOrder is not null,
                Message = foundOrder is not null ? "" : "Order could not be found."
            };
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
                return Orders
                    .Skip((page - 1) * PaginationMaxItems)
                    .Take(PaginationMaxItems)
                    .ToList();

            return Orders
                .Where(o => o.Status == status)
                .Skip((page - 1) * PaginationMaxItems)
                .Take(PaginationMaxItems)
                .ToList();
        }

        /// <summary>
        /// Updates the basic details of an order, like table number, price and status.
        /// </summary>
        /// <param name="id">The id of the order to update.</param>
        /// <param name="newDetails">The order's new details. Include the details that are not changed.</param>
        /// <returns>The details if the order could be updated. If successful, includes updated order.</returns>
        public Response<Order> UpdateOrderDetails(string id, Order newDetails)
        {
            if (newDetails.TableId < 0 || newDetails.TotalPrice < 0)
                return new Response<Order>
                {
                    Successful = false,
                    Message = "New table number and/or total price need to be greater than or equal to 0."
                };
            
            var indexOfOrderToEdit = Orders.FindIndex(order => order.Id == id);
            if (indexOfOrderToEdit < 0)
                return new Response<Order>
                {
                    Successful = false,
                    Message = "Could not find order."
                };

            Orders[indexOfOrderToEdit].TableId = newDetails.TableId;
            Orders[indexOfOrderToEdit].TotalPrice = newDetails.TotalPrice;
            Orders[indexOfOrderToEdit].Status = newDetails.Status;

            return new Response<Order>
            {
                Data = Orders[indexOfOrderToEdit]
            };
        }

        /// <summary>
        /// Adds the items to the given order id.
        /// </summary>
        /// <param name="id">The id of the order to add the items to.</param>
        /// <param name="itemsToAdd">The items that should be added to the order.</param>
        /// <returns>The details if the items could be added. If successful, includes the updated order.</returns>
        public Response<Order> AddItemsToOrder(string id, OrderItem[] itemsToAdd)
        {
            if (itemsToAdd.Any(item =>
                        string.IsNullOrWhiteSpace(item.Name) ||
                        item.Amount < 1 ||
                        item.Id < 0))
                return new Response<Order>
                {
                    Successful = false,
                    Message = "Please fill in an item name, a positive amount and an id greater than or equal to 0."
                };
            
            var indexOfOrderToEdit = Orders.FindIndex(order => order.Id == id);
            if (indexOfOrderToEdit < 0)
                return new Response<Order>
                {
                    Successful = false,
                    Message = "Could not find order."
                };

            var message = "";

            foreach (var item in itemsToAdd)
            {
                // Check if the itemToAdd Id is not already present in the items list
                if (Orders[indexOfOrderToEdit].Items.All(i => i.Id != item.Id)) 
                    Orders[indexOfOrderToEdit].Items.Add(item);
                else
                {
                    message += $" An item with name \"{item.Name}\" and id \"{item.Id}\" could not be added, because" +
                               $" its id already existed in the order.";
                }
            }

            return new Response<Order>
            {
                Data = Orders[indexOfOrderToEdit],
                Message = message
            };
        }

        /// <summary>
        /// Get an item from the order, if present.
        /// </summary>
        /// <param name="orderId">The order to search the item in.</param>
        /// <param name="itemId">The item id to search with.</param>
        /// <returns>The details if the items could be found. If successful, includes the found item.</returns>
        public Response<OrderItem> GetItemFromOrder(string orderId, int itemId)
        {
            var indexOfOrderToSearch = Orders.FindIndex(order => order.Id == orderId);
            if (indexOfOrderToSearch < 0)
                return new Response<OrderItem>
                {
                    Successful = false,
                    Message = "Could not find order."
                };

            var foundItem = Orders[indexOfOrderToSearch].Items.FirstOrDefault(item => item.Id == itemId);

            return new Response<OrderItem>
            {
                Data = foundItem,
                Successful = foundItem is not null,
                Message = foundItem is not null ? "" : "Could not find item in order."
            };
        }

        /// <summary>
        /// Deletes an item from the order.
        /// </summary>
        /// <param name="orderId">The order to delete the item from.</param>
        /// <param name="itemId">The item to delete from the order.</param>
        /// <returns>The details if the item has been removed. If successful, includes the updated order.</returns>
        public Response<Order> DeleteItemFromOrder(string orderId, int itemId)
        {
            var indexOfOrderToSearch = Orders.FindIndex(order => order.Id == orderId);
            if (indexOfOrderToSearch < 0)
                return new Response<Order>
                {
                    Successful = false,
                    Message = "Could not find order."
                };
            
            var foundItem = Orders[indexOfOrderToSearch].Items.FirstOrDefault(item => item.Id == itemId);
            if (foundItem is null)
                return new Response<Order>
                {
                    Successful = false,
                    Message = "Could not find the item."
                };
            
            Orders[indexOfOrderToSearch].Items.Remove(foundItem);
            return new Response<Order>
            {
                Data = Orders[indexOfOrderToSearch]
            };

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
