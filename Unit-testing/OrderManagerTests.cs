using System;
using Xunit;
using FluentAssertions;
using order_api;
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
        
        // Act
        _orderManager.AddOrder(input);
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
}