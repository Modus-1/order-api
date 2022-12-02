using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Text.RegularExpressions;

using WebSocketSharp;
using WebSocketSharp.Server;
using order_api.Models;
using System.Text.Json;

namespace order_api.Managers
{
    internal class NewOrderEchoService : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("Message received: " + e.Data);
        }
    }

    internal class UpdateOrderEchoService : WebSocketBehavior
    {
        private readonly IOrderManager _orderManager;
        public UpdateOrderEchoService(IOrderManager orderManager)
        {
            _orderManager = orderManager;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("Message received: " + e.Data);

            Order updatedOrder = JsonSerializer.Deserialize<Order>(e.Data);

            Sessions.Broadcast(e.Data);

            //_orderManager.UpdateOrderDetails(updatedOrder.Id, updatedOrder);
        }
    }

    public class OrderWebSocketManager : IOrderWebSocketManager
    {
        private readonly int _Port = 6969; //the port of the websocket
        private readonly WebSocketServer _wssv;

        public OrderWebSocketManager(IOrderManager orderManager)
        {
            _wssv = new WebSocketServer(_Port);
            _wssv.AddWebSocketService<NewOrderEchoService>("/order/new");
            _wssv.AddWebSocketService("order/update", () => new UpdateOrderEchoService(orderManager));
            _wssv.Start();
            Console.WriteLine("Starting websocket server on port " + _Port);
        }

        public void SendNewOrder(Order order)
        {
            string json = JsonSerializer.Serialize(order);
            _wssv.WebSocketServices["/order/new"].Sessions.Broadcast(json);
        }

        public void UpdateExistingOrder(Order order)
        {
            string json = JsonSerializer.Serialize(order);
            _wssv.WebSocketServices["/order/update"].Sessions.Broadcast(json);
        }
        
    }
}
