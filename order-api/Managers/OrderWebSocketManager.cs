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
    internal class WssNewOrderService : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            return;
        }
    }

    internal class WssUpdateOrderService : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Sessions.Broadcast(e.Data);
        }
    }

    public class OrderWebSocketManager : IOrderWebSocketManager
    {
        private readonly int _Port = 6969; //the port of the websocket
        private readonly WebSocketServer _wssv;

        public OrderWebSocketManager()
        {
            _wssv = new WebSocketServer(_Port);
            _wssv.AddWebSocketService<WssNewOrderService>("/order/new");
            _wssv.AddWebSocketService<WssUpdateOrderService>("/order/update");
            _wssv.Start();
            Console.WriteLine("Starting websocket server on port " + _Port);
        }

        private readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions //convert to camelCase for the front-end
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };


        public void SendNewOrder(Order order)
        {
            string json = JsonSerializer.Serialize(order, _serializeOptions);
            _wssv.WebSocketServices["/order/new"].Sessions.Broadcast(json);
        }

        public void UpdateExistingOrder(Order order)
        {
            string json = JsonSerializer.Serialize(order, _serializeOptions);
            _wssv.WebSocketServices["/order/update"].Sessions.Broadcast(json);
        }
        
    }
}
