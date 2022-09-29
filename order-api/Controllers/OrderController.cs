using Microsoft.AspNetCore.Mvc;
using order_api.Models;

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
        public OrderManager OrderMgr { get; set; } = new OrderManager();

        private readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Route to get the status of the specified order.
        /// </summary>
        [HttpGet("status/{id}")]
        public int GetStatus(ulong id)
        {
            Order order = OrderMgr.Get(id);
            return (int)order.Status;
        }
    }
}
