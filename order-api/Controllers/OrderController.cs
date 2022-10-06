using System.Net;
using Microsoft.AspNetCore.Mvc;
using order_api.Config;
using order_api.Models;
using order_api.Models.SchemaObjects;
using order_api.Managers;

namespace order_api.Controllers
{
    /// <summary>
    /// Order controller route.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        /// <summary>
        /// The management object to handle orders.
        /// </summary>
        private readonly IOrderManager _orderManager;

        public OrderController( IOrderManager orderManager)
        {
            _orderManager = orderManager;
        }

        /// <summary>
        /// Route to get orders.
        /// </summary>
        /// <param name="filter">Filter to use.</param>
        /// <param name="page">The page to use.</param>
        /// <returns>
        ///     200 : If the search was able to be initiated. <br/>
        ///     400 : If the filter did not exist.
        /// </returns>
        [HttpGet("active/{filter}")]
        public ActionResult GetAll(string filter = "all", int page = 1)
        {
            if (filter == "all")
                return Ok(_orderManager.GetOrderSubset(page: page));

            var correctFilter = Enum.TryParse(filter.ToUpper(), out OrderStatus status);
            if (!correctFilter)
                return BadRequest(new Response<List<Order>>
                {
                    Successful = false,
                    Message = "400: No such filter exists."
                });

            return Ok(
                _orderManager.GetOrderSubset(status, page)
            );
        }

        /// <summary>
        /// Route to create order.
        /// </summary>
        /// <param name="orderToAdd">The order to create.</param>
        /// <returns>
        ///     200 : If the order was able to be created. <br/>
        ///     400 : If the parameters were invalid.
        /// </returns>
        [HttpPost("create")]
        public ActionResult CreateOrder(PlaceOrderSchema orderToAdd)
        {
            var newOrder = new Order { TotalPrice = orderToAdd.TotalPrice, TableId = orderToAdd.TableId };
            var response = _orderManager.AddOrder(newOrder);

            return response.Successful
                ? Ok(new Response<Order?> {Data = newOrder})
                : BadRequest(new Response<Order?> {Successful = false, Message = response.Message});
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="orderId">The GUID of the order to be found.</param>
        /// <returns>
        ///     200 : If the order was found successfully. <br/>
        ///     404 : If the order was not found.
        /// </returns>
        [HttpGet("{orderId}")]
        public ActionResult GetOrder(string orderId)
        {
            var response = _orderManager.GetOrder(orderId);

            if (response.Successful && response.Data is not null)
                return Ok(response);

            return NotFound(response);
        }

        /// <summary>
        /// Deletes the specified order.
        /// </summary>
        /// <param name="orderId">The GUID of the order to delete.</param>
        /// <returns>
        ///     200 : If the order could successfully be deleted. <br/>
        ///     400 : If the order could not be deleted.
        /// </returns>
        [HttpDelete("{orderId}")]
        public ActionResult DeleteOrder(string orderId)
        {
            var success = _orderManager.DeleteOrder(orderId);

            return success
                ? Ok(new Response())
                : BadRequest(new Response {Successful = false, Message = "400: Could not delete order."});
        }

        /// <summary>
        /// Adds an item to the specified order.
        /// </summary>
        /// <param name="orderId">The order's GUID to add the item to.</param>
        /// <param name="items">The items to add to the order.</param>
        /// <returns>
        ///     200 : If the item was able to be added to the order. <br/>
        ///     404 : If the order could not be found. <br/>
        ///     400 : If the parameters were invalid.
        /// </returns>
        [HttpPost("{orderId}/item")]
        public ActionResult AddItems(string orderId, OrderItem[] items)
        {
            var response = _orderManager.AddItemsToOrder(orderId, items);

            if (response.Successful && response.Data is not null)
                return Ok(response);
            
            if (response.Message.StartsWith("404"))
                return NotFound(response);
            
            return BadRequest(response);
        }

        /// <summary>
        /// Gets the specified order item.
        /// </summary>
        /// <param name="orderId">The order's GUID of which to get the item from.</param>
        /// <param name="itemId">The id of the item to get.</param>
        /// <returns>
        ///     200 : If the items was able to be found. <br/>
        ///     404 : If either the item or the order was not found.
        /// </returns>
        [HttpGet("{orderId}/item/{itemId:int}")]
        public ActionResult GetItem(string orderId, int itemId)
        {
            var response = _orderManager.GetItemFromOrder(orderId, itemId);

            if (response.Successful && response.Data is not null)
                return Ok(response);

            return NotFound(response);
        }

        /// <summary>
        /// Deletes the specified order item.
        /// </summary>
        /// <param name="orderId">The GUID of the order of which to delete the item from</param>
        /// <param name="itemId">The ID of the item to delete</param>
        /// <returns>
        ///     200 : If the item was successfully removed from the order. <br/>
        ///     404 : If the item or the order was not found.
        /// </returns>
        [HttpDelete("{orderId}/item/{itemId:int}")]
        public ActionResult DeleteItem(string orderId, int itemId)
        {
            var response = _orderManager.DeleteItemFromOrder(orderId, itemId);

            if (response.Successful && response.Data is not null)
                return Ok(response);

            return NotFound(response);
        }

        /// <summary>
        /// Updates an order's basic details like table number, total price and order status.
        /// </summary>
        /// <param name="orderId">The GUID of the order to be updated.</param>
        /// <param name="newDetails">The new order details. Include the unchanged details as well.</param>
        /// <returns>
        ///     200 : If the order was successfully updated. <br/>
        ///     404 : If the order was not found. <br/>
        ///     400 : If the parameters were invalid.
        /// </returns>
        [HttpPut("{orderId}")]
        public ActionResult UpdateOrder(string orderId, Order newDetails)
        {
            var response = _orderManager.UpdateOrderDetails(orderId, newDetails);

            if (response.Successful && response.Data is not null)
                return Ok(response);

            if (response.Message.StartsWith("404"))
                return NotFound(response);
            
            return BadRequest(response);
        }

        /// <summary>
        /// Finalizes the order. In other words:
        ///   - The order is complete
        ///   - It is removed from the memory list
        ///   - The order is dumped to the database for record keeping.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>
        ///     200 : If the order was successfully finalized. <br />
        ///     404 : If the order was not found. <br />
        ///     400 : If the order was not able to be finalized.
        /// </returns>
        [HttpPost("{orderId}/finalize")]
        public async Task<ActionResult> FinalizeOrder(string orderId)
        {
            try
            {
                var response = _orderManager.GetOrder(orderId);

                if (!response.Successful || response.Data is null)
                    return NotFound(response);

                var order = response.Data;

                // Step 1. Set order status to DONE
                // It is important clients ignore orders which are "DONE", because it implies there is nothing further to be done.
                order.Status = OrderStatus.DONE;

                // Step 2. Store a copy of this order in the database
                // If no database URI is specified, do not store anything

                if (!string.IsNullOrEmpty(DatabaseConfiguration.DATABASE_URI))
                    await _orderManager.SaveOrderToDatabase(order);

                // Finally, delete the order
                _orderManager.DeleteOrder(orderId);
            }
            catch
            {
                return BadRequest(
                    new Response
                    {
                        Message = "Could not finalize order successfully.",
                        Successful = false
                    });
            }

            return Ok(new { success = true });
        }
    }
}
