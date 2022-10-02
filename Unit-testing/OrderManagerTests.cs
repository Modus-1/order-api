using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using order_api;
using order_api.Managers;
using order_api.Models;

namespace Unit_testing;

public class OrderManagerTests
{
    private OrderManager _orderManager;
    
    public OrderManagerTests()
    {
        _orderManager = new OrderManager();
    }

    [Fact]
    public void AddOrder_WithNegativeTableId_ShouldReturnANegativeResponse()
    {
        // Arrange
        var input = new Order {TableId = -1};
        var response = new Response();

        // Act
        var action = () => response = _orderManager.AddOrder(input);

        // Assert
        action
            .Should()
            .NotThrow();
        response.Successful
            .Should()
            .BeFalse();
    }

    [Fact]
    public void AddOrder_WithNegativeTotalPrice_ShouldReturnANegativeResponse()
    {
        // Arrange
        var input = new Order {TotalPrice = -1.00m};
        var response = new Response();

        // Act
        var action = () => response = _orderManager.AddOrder(input);

        // Assert
        action
            .Should()
            .NotThrow();
        response.Successful
            .Should()
            .BeFalse();
    }

    [Fact]
    public void AddOrder_WithDuplicateOrder_ShouldReturnANegativeResponse()
    {
        // Arrange 
        var input = new Order();
        _orderManager.AddOrder(input);
        var response = new Response();
        
        // Act
        var action = () => response = _orderManager.AddOrder(input); // try again with same order

        // Assert
        action
            .Should()
            .NotThrow();
        response.Successful
            .Should()
            .BeFalse();
    }

    [Fact]
    public void AddOrder_WithNormalOrder_ShouldPopulateOrderList()
    {
        // Arrange
        var input = new Order();
        var response = new Response();
        
        // Act
        var action = () => response = _orderManager.AddOrder(input);

        // Assert
        action
            .Should()
            .NotThrow();
        response.Successful
            .Should()
            .BeTrue();
        _orderManager.Orders
            .Should()
            .NotBeEmpty()
            .And.HaveCount(1)
            .And.Contain(input);
    }

    [Fact]
    public void DeleteOrder_WithEmptyOrderList_ShouldReturnFalse()
    {
        // Arrange
        var input = Guid.NewGuid().ToString();
        var success = false;

        // Act
        var action = () => success = _orderManager.DeleteOrder(input);

        // Assert
        action
            .Should()
            .NotThrow();
        success
            .Should()
            .BeFalse();
        _orderManager.Orders
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void DeleteOrder_WithCorrectOrder_ShouldRemoveOrder()
    {
        // Arrange
        var order = new Order();
        _orderManager.AddOrder(order);
        _orderManager.AddOrder(new Order());
        var success = false;
        
        // Act
        var action = () => success =_orderManager.DeleteOrder(order.Id);

        // Assert
        action
            .Should()
            .NotThrow();
        success
            .Should()
            .BeTrue();
        _orderManager.Orders
            .Should()
            .NotBeEmpty()
            .And.HaveCount(1);
    }

    [Fact]
    public void GetOrder_WithPopulatedOrderList_ShouldRetrieveOrder()
    {
        // Arrange
        var order = new Order();
        _orderManager.AddOrder(order);
        var response = new Response<Order>();
        
        // Act
        var action = () => response = _orderManager.GetOrder(order.Id);

        // Assert
        action
            .Should()
            .NotThrow();
        response
            .Successful
            .Should()
            .BeTrue();
        response.Data
            .Should()
            .NotBeNull()
            .And.Be(order);
    }

    [Fact]
    public void GetOrder_WhenNotFound_ShouldReturnANegativeResponse()
    {
        // Arrange
        var response = new Response<Order>();
        
        // Act
        var action = () => response = _orderManager.GetOrder(Guid.NewGuid().ToString());

        // Assert
        action
            .Should()
            .NotThrow();
        response.Successful
            .Should()
            .BeFalse();
        response.Data
            .Should()
            .BeNull();
    }

    [Fact]
    public void GetOrderSubset_WithEmptyOrderList_ShouldReturnEmptyOrderList()
    {
        // Arrange
        var results = new List<Order>();
        
        // Act
        var action = () => results = _orderManager.GetOrderSubset();
        
        // Assert
        action
            .Should()
            .NotThrow();
        results
            .Should()
            .NotBeNull()
            .And.BeEmpty();
    }

    [Fact]
    public void GetOrderSubset_WithPopulatedOrderList_ShouldReturnCorrectAmountOfOrders()
    {
        // Arrange
        const int collectionSize = 5;
        for (var i = 0; i < collectionSize; i++)
        {
            _orderManager.AddOrder(new Order
                {
                    Status = OrderStatus.PLACED
                }
            );
        }

        var results = new List<Order>();

        // Act
        var action = () => results = _orderManager.GetOrderSubset(OrderStatus.PLACED);

        // Assert
        action
            .Should()
            .NotThrow();
        results
            .Should()
            .NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(collectionSize)
            .And.OnlyContain(order => order.Status == OrderStatus.PLACED);
    }

    [Fact]
    public void GetOrderSubset_WithMixedOrderList_ShouldReturnCorrectAmountOfOrders()
    {
        // Arrange
        const int collectionSize = 10;
        const int expectedReturnCollectionSize = 5;
        for (var i = 1; i < collectionSize + 1; i++)
        {
            var status = i % 2 == 0 ? OrderStatus.PLACED : OrderStatus.PROCESSING;
            _orderManager.AddOrder(new Order
            {
                Id = i.ToString(),
                Status = status
            });
        }

        var results = new List<Order>();

        // Act
        var action = () => results = _orderManager.GetOrderSubset(OrderStatus.PLACED);

        // Assert
        action
            .Should()
            .NotThrow();
        results
            .Should()
            .NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(expectedReturnCollectionSize)
            .And.OnlyContain(order => Convert.ToInt16(order.Id) % 2 == 0)
            .And.OnlyContain(order => order.Status == OrderStatus.PLACED);
    }

    [Fact]
    public void GetOrderSubset_WithLargeList_ShouldReturnCorrectOrderListPage()
    {
        // Arrange
        const int collectionSize = 20;
        const int expectedReturnCollectionSize = 10;
        for (var i = 1; i < collectionSize + 1; i++)
        {
            _orderManager.AddOrder(new Order
            {
                Id = i.ToString(),
                Status = OrderStatus.PLACED
            });
        }

        var results = new List<Order>();

        // Act
        var action = () => results = _orderManager.GetOrderSubset(OrderStatus.PLACED, 2);
        
        // Assert
        action
            .Should()
            .NotThrow();
        results
            .Should()
            .NotBeNull()
            .And.NotBeEmpty()
            .And.OnlyContain(order => order.Status == OrderStatus.PLACED)
            .And.OnlyContain(order => Convert.ToInt16(order.Id) > 10);
    }
}