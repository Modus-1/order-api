using System;
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
    public void AddOrder_WithNullOrder_ShouldThrowException()
    {
        // Arrange
        Order? input = null;

        // Act
        var action = () => _orderManager.AddOrder(input!);

        // Assert
        action
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOrder_WithNegativeTableId_ShouldThrowException()
    {
        // Arrange
        var input = new Order {TableId = -1};

        // Act
        var action = () => _orderManager.AddOrder(input);

        // Assert
        action
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddOrder_WithNegativeTotalPrice_ShouldThrowException()
    {
        // Arrange
        var input = new Order {TotalPrice = -1.00m};

        // Act
        var action = () => _orderManager.AddOrder(input);

        // Assert
        action
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddOrder_WithDuplicateOrder_ShouldThrowException()
    {
        // Arrange 
        var input = new Order();
        _orderManager.AddOrder(input);
        
        // Act
        var action = () => _orderManager.AddOrder(input); // try again with same order

        // Assert
        action
            .Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void AddOrder_WithNormalOrder_ShouldPopulateOrderList()
    {
        // Arrange
        var input = new Order();
        
        // Act
        _orderManager.AddOrder(input);

        // Assert
        _orderManager.Orders
            .Should()
            .NotBeEmpty()
            .And.HaveCount(1)
            .And.Contain(input);
    }

    [Fact]
    public void DeleteOrder_WithNullInput_ShouldThrowException()
    {
        // Arrange
        string? input = null;
        
        // Act
        var action = () => _orderManager.DeleteOrder(input!);

        // Assert
        action
            .Should()
            .Throw<ArgumentNullException>();
        _orderManager.Orders
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void DeleteOrder_WithEmptyOrderList_ShouldThrowException()
    {
        // Arrange
        var input = Guid.NewGuid().ToString();

        // Act
        var action = () => _orderManager.DeleteOrder(input);

        // Assert
        action
            .Should()
            .Throw<ArgumentNullException>();
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
        
        // Act
        var action = () => _orderManager.DeleteOrder(order.Id);

        // Assert
        action
            .Should()
            .NotThrow();
        _orderManager.Orders
            .Should()
            .NotBeEmpty()
            .And.HaveCount(1);
    }

    [Fact]
    public void Get_WithPopulatedOrderList_ShouldRetrieveOrder()
    {
        // Arrange
        var order = new Order();
        _orderManager.AddOrder(order);
        Order? result = null;
        
        // Act
        var action = () => result = _orderManager.GetOrder(order.Id);

        // Assert
        action
            .Should()
            .NotThrow();
        result
            .Should()
            .NotBeNull()
            .And.Be(order);
    }

    [Fact]
    public void Get_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        Order? result = null;
        
        // Act
        var action = () => result = _orderManager.GetOrder(Guid.NewGuid().ToString());

        // Assert
        action
            .Should()
            .NotThrow();
        result
            .Should()
            .BeNull();
    }
}