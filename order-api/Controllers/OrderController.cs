using Microsoft.AspNetCore.Mvc;
using order_api.Models;
using order_api.Models.SchemaObjects;

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
        public static OrderManager OrderMgr { get; set; } = new OrderManager();

        private readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Route to get orders.
        /// </summary>
        /// <param name="filter">Filter to use.</param>
        /// <returns></returns>
        [HttpGet("active/{filter}")]
        public ActionResult GetAll(string filter = "all")
        {
            List<Order> orders;

            switch(filter)
            {
                case "all":
                    orders = OrderMgr.Orders;
                    break;
                case "placed":
                    orders = OrderMgr.Orders.FindAll((x) => x.Status == OrderStatus.PLACED);
                    break;
                case "processing":
                    orders = OrderMgr.Orders.FindAll((x) => x.Status == OrderStatus.PROCESSING);
                    break;
                case "ready":
                    orders = OrderMgr.Orders.FindAll((x) => x.Status == OrderStatus.READY);
                    break;
                default:
                    return BadRequest();
            }

            return Ok(orders);
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
                OrderMgr.AddOrder(orderToAdd);
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
            Order? o = OrderMgr.Get(guid);

            if (o == null)
                return BadRequest(new { message = "Order not found." });

            return Ok(o);
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
            Order? o = OrderMgr.Get(guid);

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
        /// Route to get the status of the specified order.
        /// </summary>
        [HttpGet("{guid}/status")]
        public ActionResult GetStatus(string guid)
        {
            Order order;

            try
            {
                order = OrderMgr.Get(guid);
            }
            catch(Exception)
            {
                return BadRequest();
            }

            return Ok(new
            {
                status = (int)order.Status
            });
        }
    }
}
