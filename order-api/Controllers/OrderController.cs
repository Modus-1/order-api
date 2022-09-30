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
        /// <param name="o">The order to create.</param>
        /// <returns></returns>
        [HttpPost("create")]
        public ActionResult CreateOrder(PlaceOrderSchema o)
        {
            try
            {
                var orderToAdd = new Order { TotalPrice = o.TotalPrice, TableId = o.TableId };
                _orderManager.AddOrder(orderToAdd);
                return Ok(orderToAdd);
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    message = e.Message
                });
            }
        }

        /// <summary>
        /// Gets the order state.
        /// </summary>
        /// <param name="guid">Order ID.</param>
        /// <returns></returns>
        [HttpGet("{guid}")]
        public ActionResult GetOrder(string guid)
        {
            Order? o = _orderManager.GetOrder(guid);

            if (o == null)
                return BadRequest(new { message = "Order not found." });

            return Ok(o);
        }

        /// <summary>
        /// Deletes the specified order.
        /// </summary>
        /// <param name="guid">The GUID of the order to delete.</param>
        /// <returns></returns>
        [HttpDelete("{guid}")]
        public ActionResult DeleteOrder(string guid)
        {
            try
            {
                Order? order = _orderManager.GetOrder(guid);

                if (order == null)
                    return BadRequest(new { message = "Order not found." });

                _orderManager.DeleteOrder(guid);
            }
            catch(Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

            return Ok(new { success = true });
        }

        /// <summary>
        /// Adds an item to the specified order.
        /// </summary>
        /// <param name="guid">Order ID.</param>
        /// <param name="items">Items to add.</param>
        /// <returns></returns>
        [HttpPost("{guid}/item")]
        public ActionResult AddItems(string guid, OrderItem[] items)
        {
            Order? o = _orderManager.GetOrder(guid);

            if (o == null)
                return BadRequest(new { message = "Order not found." });

            try
            {
                foreach (OrderItem itm in items)
                    o.AddItem(itm);
            }
            catch(Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

            return Ok(o);
        }

        /// <summary>
        /// Gets the specified order item.
        /// </summary>
        /// <param name="guid">The GUID of the order.</param>
        /// <param name="id">The ID of the item.</param>
        /// <returns></returns>
        [HttpGet("{guid}/item/{id}")]
        public ActionResult GetItem(string guid, int id)
        {
            Order? order = _orderManager.GetOrder(guid);

            if (order == null)
                return BadRequest(new { message = "Order not found." });

            OrderItem? item = order.GetItem(id);

            if (item == null)
                return BadRequest(new { message = "Item not found." });

            return Ok(item);
        }

        /// <summary>
        /// Deletes the specified order item.
        /// </summary>
        /// <param name="guid">The GUID of the order.</param>
        /// <param name="id">The ID of the item.</param>
        /// <returns></returns>
        [HttpDelete("{guid}/item/{id}")]
        public ActionResult DeleteItem(string guid, int id)
        {
            Order? order = _orderManager.GetOrder(guid);

            if (order == null)
                return BadRequest(new { message = "Order not found." });

            OrderItem? item = order.GetItem(id);

            if (item == null)
                return BadRequest(new { message = "Item not found." });

            order.RemoveItem(id);

            return Ok(order);
        }

        /// <summary>
        /// Route to get the status of the specified order.
        /// </summary>
        [HttpGet("{guid}/status")]
        public ActionResult GetStatus(string guid)
        {
            Order? order = _orderManager.GetOrder(guid);

            if (order == null)
                return BadRequest(new { message = "Order not found." });

            return Ok(new
            {
                status = (int)order.Status
            });
        }

        /// <summary>
        /// Sets the order status.
        /// </summary>
        /// <param name="guid">The order to set the status for.</param>
        /// <param name="status">The status to set.</param>
        /// <returns></returns>
        [HttpPut("{guid}/status")]
        public ActionResult SetStatus(string guid, int status)
        {
            Order? order = _orderManager.GetOrder(guid);

            if (order == null)
                return BadRequest(new { message = "Order not found." });

            try
            {
                var orderStatus = (OrderStatus)status;

                // Validate status
                if (!Enum.IsDefined(typeof(OrderStatus), orderStatus))
                    throw new Exception("Status is not valid.");

                order.Status = orderStatus;
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

            return Ok(order);
        }

        /// <summary>
        /// Sets the order price.
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        [HttpPut("{guid}/price")]
        public ActionResult SetPrice(string guid, decimal price)
        {
            Order? order = _orderManager.GetOrder(guid);

            if (order == null)
                return BadRequest(new { message = "Order not found." });

            try
            {
                order.SetPrice(price);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

            return Ok(order);
        }

        /// <summary>
        /// Route to get the table number of the specified order.
        /// </summary>
        [HttpGet("{guid}/tableno")]
        public ActionResult GetTableId(string guid)
        {
            Order? order = _orderManager.GetOrder(guid);

            if (order == null)
                return BadRequest(new { message = "Order not found." });

            return Ok(new
            {
                tableId = order.TableId
            });
        }

        /// <summary>
        /// Route to get the price of the specified order.
        /// </summary>
        [HttpGet("{guid}/price")]
        public ActionResult GetPrice(string guid)
        {
            Order? order = _orderManager.GetOrder(guid);

            if (order == null)
                return BadRequest(new { message = "Order not found." });

            return Ok(new
            {
                totalPrice = order.TotalPrice
            });
        }

        /// <summary>
        /// Sets the table number.
        /// </summary>
        /// <param name="tableNo">The table number</param>
        /// <returns></returns>
        [HttpPut("{guid}/tableno")]
        public ActionResult SetTableNo(string guid, int tableNo)
        {
            Order? order = _orderManager.GetOrder(guid);

            if (order == null)
                return BadRequest(new { message = "Order not found." });

            try
            {
                order.SetTable(tableNo);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

            return Ok(order);
        }

        /// <summary>
        /// Finalizes the order. In other words:
        ///   - The order is complete
        ///   - It is removed from the memory list
        ///   - The order is dumped to the database for record keeping.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpPost("{guid}/finalize")]
        public async Task<ActionResult> FinalizeOrder(string guid)
        {
            try
            {
                Order? order = _orderManager.GetOrder(guid);

                if (order == null)
                    return BadRequest(new { message = "Order not found." });

                // Step 1. Set order status to DONE
                // It is important clients ignore orders which are "DONE", because it implies there is nothing further to be done.
                order.Status = OrderStatus.DONE;

                // Step 2. Store a copy of this order in the database
                // If no database URI is specified, do not store anything

                if (!string.IsNullOrEmpty(DatabaseConfiguration.DATABASE_URI))
                    await _orderManager.SaveOrderToDatabase(order);

                // Finally, delete the order
                _orderManager.DeleteOrder(guid);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

            return Ok(new { success = true });
        }
    }
}
