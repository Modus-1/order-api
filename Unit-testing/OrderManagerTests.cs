using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using order_api;
using order_api.Managers;
using order_api.Models;

namespace Unit_testing;

public class OrderManagerTests
{
    private readonly OrderManager _orderManager;
    
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
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
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
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
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
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
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
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        _orderManager.Orders.Should().NotBeEmpty()
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
        action.Should().NotThrow();
        success.Should().BeFalse();
        _orderManager.Orders.Should().BeEmpty();
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
        action.Should().NotThrow();
        success.Should().BeTrue();
        _orderManager.Orders.Should().NotBeEmpty()
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
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
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
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
    }

    [Fact]
    public void GetOrderSubset_WithEmptyOrderList_ShouldReturnEmptyOrderList()
    {
        // Arrange
        var response = new Response<List<Order>>();
        
        // Act
        var action = () => response = _orderManager.GetOrderSubset();
        
        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
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

        var response = new Response<List<Order>>();

        // Act
        var action = () => response = _orderManager.GetOrderSubset(OrderStatus.PLACED);

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
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

        var response = new Response<List<Order>>();

        // Act
        var action = () => response = _orderManager.GetOrderSubset(OrderStatus.PLACED);

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
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

        var response = new Response<List<Order>>();

        // Act
        var action = () => response = _orderManager.GetOrderSubset(OrderStatus.PLACED, 2);
        
        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(expectedReturnCollectionSize)
            .And.OnlyContain(order => order.Status == OrderStatus.PLACED)
            .And.OnlyContain(order => Convert.ToInt16(order.Id) > 10);
    }

    [Fact]
    public void GetOrderSubset_WithNegativePageParameter_ShouldReturnFirstPage()
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

        var response = new Response<List<Order>>();
        
        // Act
        var action = () => response = _orderManager.GetOrderSubset(OrderStatus.PLACED, -1);
        
        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(collectionSize)
            .And.OnlyContain(order => order.Status == OrderStatus.PLACED);
    }
    
    [Fact]
    public void UpdateOrderDetails_WithNegativeTableId_ShouldReturnANegativeResponse()
    {
        // Arrange
        var expectedOrderResult = new Order
        {
            TableId = 1
        };
        _orderManager.AddOrder(expectedOrderResult);
        var response = new Response<Order>();

        // Act
        var action = () => response = _orderManager.UpdateOrderDetails(expectedOrderResult.Id, new Order
        {
            Id = expectedOrderResult.Id,
            TableId = -1
        });
        var finalOrder = _orderManager.GetOrder(expectedOrderResult.Id).Data;

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
        finalOrder.Should().NotBeNull()
            .And.BeEquivalentTo(expectedOrderResult);

    }
    
    [Fact]
    public void UpdateOrderDetails_WithNegativeOrderPrice_ShouldReturnANegativeResponse()
    {
        // Arrange
        var expectedResult = new Order
        {
            TotalPrice = 42m
        };
        _orderManager.AddOrder(expectedResult);
        var response = new Response<Order>();

        // Act
        var action = () => response = _orderManager.UpdateOrderDetails(expectedResult.Id, new Order
        {
            Id = expectedResult.Id,
            TotalPrice = -1
        });
        var finalOrder = _orderManager.GetOrder(expectedResult.Id).Data;

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
        finalOrder.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void UpdateOrderDetails_WhenOrderCannotBeFound_ShouldReturnANegativeResponse()
    {
        // Arrange
        var response = new Response<Order>();
        
        // Act
        var action = () => response = _orderManager.UpdateOrderDetails("fake", new Order
        {
            Id = "fake",
            TableId = 42
        });

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public void UpdateOrderDetails_WhenOrderCanBeSuccessfullyUpdated_ShouldReturnEditedOrder()
    {
        // Arrange
        var expectedResult = new Order
        {
            Id = "Test",
            TableId = 42,
            TotalPrice = 69m,
            CreationTime = DateTime.Today
        };
        _orderManager.AddOrder(new Order {Id = expectedResult.Id, CreationTime = DateTime.Today});
        var response = new Response<Order>();
        
        // Act
        var action = () => response = _orderManager.UpdateOrderDetails(expectedResult.Id, expectedResult);
        var finalOrder = _orderManager.GetOrder(expectedResult.Id).Data;

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
        finalOrder.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void AddItemsToOrder_WithEmptyName_ShouldReturn_ShouldReturnANegativeResponse()
    {
        // Arrange
        var orderToAddItemsIn = new Order {Id = "test"};
        _orderManager.AddOrder(orderToAddItemsIn);
        var itemsToAdd = new [] {new OrderItem {Name = string.Empty}};
        var response = new Response<Order>();
        
        // Act
        var action = () => response = _orderManager.AddItemsToOrder(orderToAddItemsIn.Id, itemsToAdd);
        var finalOrder = _orderManager.GetOrder(orderToAddItemsIn.Id).Data;

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
        finalOrder.Should().NotBeNull();
        finalOrder?.Items.Should().BeEmpty();
    }
    
    [Fact]
    public void AddItemsToOrder_WithNegativeAmount_ShouldReturnANegativeResponse()
    {
        // Arrange
        var orderToAddItemsIn = new Order {Id = "test"};
        _orderManager.AddOrder(orderToAddItemsIn);
        var itemsToAdd = new [] {new OrderItem {Name = "test", Amount = -1, Id = 1}};
        var response = new Response<Order>();

        // Act
        var action = () => response = _orderManager.AddItemsToOrder(orderToAddItemsIn.Id, itemsToAdd);
        var finalOrder = _orderManager.GetOrder(orderToAddItemsIn.Id).Data;
        
        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
        finalOrder.Should().NotBeNull();
        finalOrder?.Items.Should().BeEmpty();
    }
    
    [Fact]
    public void AddItemsToOrder_WithNegativeId_ShouldReturnANegativeResponse()
    {
        // Arrange
        var orderToAddItemsIn = new Order {Id = "test"};
        _orderManager.AddOrder(orderToAddItemsIn);
        var itemsToAdd = new [] {new OrderItem {Name = "test", Id = -1, Amount = 1}};
        var response = new Response<Order>();

        // Act
        var action = () => response = _orderManager.AddItemsToOrder(orderToAddItemsIn.Id, itemsToAdd);
        var finalOrder = _orderManager.GetOrder(orderToAddItemsIn.Id).Data;

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
        finalOrder.Should().NotBeNull();
        finalOrder?.Items.Should().BeEmpty();
    }
    
    [Fact]
    public void AddItemsToOrder_WhenOrderCannotBeFound_ShouldReturnANegativeResponse()
    {
        // Arrange
        var itemsToAdd = new [] {new OrderItem {Name = "test", Id = 1, Amount = 1}};
        var response = new Response<Order>();
        
        // Act
        var action = () => response = _orderManager.AddItemsToOrder("dud", itemsToAdd);

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public void AddItemsToOrder_WhenOrderItemIsAlreadyPresent_ShouldSkipThatItem()
    {
        // Arrange
        var orderToAddItemsIn = new Order {Id = "test", CreationTime = DateTime.Today};
        _orderManager.AddOrder(orderToAddItemsIn);
        var startingItems = new[]
        {
            new OrderItem {Name = "test1", Id = 1, Amount = 1}, 
            new OrderItem {Name = "test2", Id = 2, Amount = 1}
        };
        var expectedResult = new Order
        {
            Id = "test", 
            CreationTime = DateTime.Today, 
            Items = (new []
            {
                new OrderItem {Name = "test1",Id = 1, Amount = 1}, 
                new OrderItem {Name = "test2",Id = 2, Amount = 1}, 
                new OrderItem {Name = "test3", Id = 3, Amount = 1}
            }).ToList()
        };
        _orderManager.AddItemsToOrder(orderToAddItemsIn.Id, startingItems);
        var itemsToAdd = new []
        {
            new OrderItem {Name = "test2", Id = 2, Amount = 1}, 
            new OrderItem {Name = "test3", Id = 3, Amount = 1}
        };
        var response = new Response<Order>();

        // Act
        var action = () => response = _orderManager.AddItemsToOrder(orderToAddItemsIn.Id, itemsToAdd);
        var finalOrder = _orderManager.GetOrder(orderToAddItemsIn.Id).Data;

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Message.Should().NotBeEmpty();
        response.Data.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
        finalOrder.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void AddItemsToOrder_WhenItemCanBeAddedSuccessfully_ShouldReturnEditedOrder()
    {
        // Arrange
        var orderToAddItemsIn = new Order {Id = "test", CreationTime = DateTime.Today};
        var itemsToAdd = new[] {new OrderItem {Name = "testItem", Id = 1, Amount = 1}};
        _orderManager.AddOrder(orderToAddItemsIn);
        var expectedResult = new Order
        {
            Id = "test", 
            CreationTime = DateTime.Today, 
            Items = new[]
            {
                new OrderItem {Name = "testItem", Id = 1, Amount = 1}
            }.ToList()
        };
        var response = new Response<Order>();
        
        // Act
        var action = () => response = _orderManager.AddItemsToOrder(orderToAddItemsIn.Id, itemsToAdd);
        var finalOrder = _orderManager.GetOrder(orderToAddItemsIn.Id).Data;

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Message.Should().BeEmpty();
        response.Data.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
        finalOrder.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
    }
            
    [Fact]
    public void GetItemFromOrder_WhenOrderCannotBeFound_ShouldReturnANegativeResponse()
    {
        // Arrange
        const int itemIdToSearchFor = 1;
        var response = new Response<OrderItem>();

        // Act
        var action = () => response = _orderManager.GetItemFromOrder("dud", itemIdToSearchFor);

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public void GetItemFromOrder_WhenItemCannotBeFound_ShouldReturnANegativeResponse()
    {
        // Arrange
        var orderToGetItemsFrom = new Order {Id = "test"};
        _orderManager.AddOrder(orderToGetItemsFrom);
        var response = new Response<OrderItem>();

        // Act
        var action = () => response = _orderManager.GetItemFromOrder(orderToGetItemsFrom.Id, 1);

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public void GetItemFromOrder_WhenItemCanSuccessfullyBeFound_ShouldReturnItem()
    {
        // Arrange
        var orderToGetItemsFrom = new Order {Id = "test"};
        _orderManager.AddOrder(orderToGetItemsFrom);
        var expectedResult = new OrderItem {Name = "testItem", Id = 1, Amount = 1};
        _orderManager.AddItemsToOrder(orderToGetItemsFrom.Id, new[] {expectedResult});
        var response = new Response<OrderItem>();

        // Act
        var action = () => response = 
            _orderManager.GetItemFromOrder(orderToGetItemsFrom.Id, expectedResult.Id);

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void DeleteItemFromOrder_WhenOrderCannotBeFound_ShouldReturnANegativeResponse()
    {
        // Arrange
        const int itemIdToDelete = 1;
        var response = new Response<Order>();

        // Act
        var action = () => response = _orderManager.DeleteItemFromOrder("dud", itemIdToDelete);

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public void DeleteItemFromOrder_WhenItemCannotBeFound_ShouldReturnANegativeResponse()
    {
        // Arrange
        var orderToFindTheItemIn = new Order {Id = "test"};
        _orderManager.AddOrder(orderToFindTheItemIn);
        var response = new Response<Order>();

        // Act
        var action = () => response = _orderManager.DeleteItemFromOrder(orderToFindTheItemIn.Id, 1);

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeFalse();
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public void DeleteItemFromOrder_WhenAbleToSuccessfullyDeleteTheItem_ShouldReturnTheOrderWithoutSaidItem()
    {
        // Arrange
        var orderToRemoveItemsFrom = new Order {Id = "test", CreationTime = DateTime.Today};
        _orderManager.AddOrder(orderToRemoveItemsFrom);
        var itemsToAdd = new[]
        {
            new OrderItem {Name = "test1", Id = 1, Amount = 1},
            new OrderItem {Name = "test2", Id = 2, Amount = 1}
        };
        _orderManager.AddItemsToOrder(orderToRemoveItemsFrom.Id, itemsToAdd);
        const int itemIdToRemove = 2;
        var expectedResult = new Order
        {
            Id = "test",
            CreationTime = DateTime.Today,
            Items = new[]
            {
                itemsToAdd[0]
            }.ToList()
        };
        var response = new Response<Order>();

        // Act
        var action = () => response = _orderManager.DeleteItemFromOrder(orderToRemoveItemsFrom.Id, itemIdToRemove);
        var finalOrder = _orderManager.GetOrder(orderToRemoveItemsFrom.Id).Data;

        // Assert
        action.Should().NotThrow();
        response.Successful.Should().BeTrue();
        response.Data.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
        finalOrder.Should().NotBeNull()
            .And.BeEquivalentTo(expectedResult);
    }
}