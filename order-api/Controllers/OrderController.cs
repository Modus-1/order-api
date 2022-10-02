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

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger, IOrderManager orderManager)
        {
            _logger = logger;
            _orderManager = orderManager;
        }

        /// <summary>
        /// Route to get orders.
        /// </summary>
        /// <param name="filter">Filter to use.</param>
        /// <param name="page">The page to use.</param>
        /// <returns></returns>
        [HttpGet("active/{filter}")]
        public ActionResult GetAll(string filter = "all", int page = 1)
        {
            if (filter == "all")
                return Ok(_orderManager.GetOrderSubset(page: page));

            var correctFilter = Enum.TryParse(filter.ToUpper(), out OrderStatus status);
            if (!correctFilter)
                return BadRequest("No such filter exists.");

            return Ok(
                _orderManager.GetOrderSubset(status, page)
            );
        }

        /// <summary>
        /// Route to create order.
        /// </summary>
        /// <param name="orderToAdd">The order to create.</param>
        /// <returns></returns>
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
        /// Gets the order state.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <returns></returns>
        [HttpGet("{orderId}")]
        public ActionResult GetOrder(string orderId)
        {
            var response = _orderManager.GetOrder(orderId);

            if (response.Successful && response.Data is not null)
                return Ok(response);

            return BadRequest(response);
        }

        /// <summary>
        /// Deletes the specified order.
        /// </summary>
        /// <param name="orderId">The GUID of the order to delete.</param>
        /// <returns></returns>
        [HttpDelete("{orderId}")]
        public ActionResult DeleteOrder(string orderId)
        {
            var success = _orderManager.DeleteOrder(orderId);

            return success
                ? Ok(new Response())
                : BadRequest(new Response {Successful = false, Message = "Could not delete order."});
        }

        /// <summary>
        /// Adds an item to the specified order.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <param name="items">Items to add.</param>
        /// <returns></returns>
        [HttpPost("{orderId}/item")]
        public ActionResult AddItems(string orderId, OrderItem[] items)
        {
            var response = _orderManager.AddItemsToOrder(orderId, items);

            if (response.Successful && response.Data is not null)
                return Ok(response);
            
            return BadRequest(response);
        }

        /// <summary>
        /// Gets the specified order item.
        /// </summary>
        /// <param name="orderId">The GUID of the order.</param>
        /// <param name="itemId">The ID of the item.</param>
        /// <returns></returns>
        [HttpGet("{orderId}/item/{itemId:int}")]
        public ActionResult GetItem(string orderId, int itemId)
        {
            var response = _orderManager.GetItemFromOrder(orderId, itemId);

            if (response.Successful && response.Data is not null)
                return Ok(response);

            return BadRequest(response);
        }

        /// <summary>
        /// Deletes the specified order item.
        /// </summary>
        /// <param name="orderId">The GUID of the order.</param>
        /// <param name="itemId">The ID of the item.</param>
        /// <returns></returns>
        [HttpDelete("{orderId}/item/{itemId:int}")]
        public ActionResult DeleteItem(string orderId, int itemId)
        {
            var response = _orderManager.DeleteItemFromOrder(orderId, itemId);

            if (response.Successful && response.Data is not null)
                return Ok(response);

            return BadRequest(response);
        }

        /// <summary>
        /// Updates an order's basic details like table number, total price and order status.
        /// </summary>
        /// <param name="orderId">The GUID of the order.</param>
        /// <param name="newDetails">The new order details. Include the unchanged details as well.</param>
        /// <returns></returns>
        [HttpPut("{orderId}")]
        public ActionResult UpdateOrder(string orderId, Order newDetails)
        {
            var response = _orderManager.UpdateOrderDetails(orderId, newDetails);

            if (response.Successful && response.Data is not null)
                return Ok(response);

            return BadRequest(response);
        }

        /// <summary>
        /// Finalizes the order. In other words:
        ///   - The order is complete
        ///   - It is removed from the memory list
        ///   - The order is dumped to the database for record keeping.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost("{orderId}/finalize")]
        public async Task<ActionResult> FinalizeOrder(string orderId)
        {
            try
            {
                var response = _orderManager.GetOrder(orderId);

                if (!response.Successful || response.Data is null)
                    return BadRequest(response);

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
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

            return Ok(new { success = true });
        }
    }
}
