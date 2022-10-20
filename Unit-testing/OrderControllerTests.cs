using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using order_api;
using order_api.Controllers;
using order_api.Managers;
using order_api.Models;
using order_api.Models.SchemaObjects;
using Xunit;

namespace Unit_testing;

public class OrderControllerTests
{
    private readonly Mock<IOrderManager> _mockOrderManager;
    private readonly OrderController _orderController;
    private const string TestId = "test";

    public OrderControllerTests()
    {
        _mockOrderManager = new Mock<IOrderManager>();
        _orderController = new OrderController(_mockOrderManager.Object);
    }

    [Fact]
    public void GetAll_WithAllFilter_ShouldReturnOK()
    {
        // Arrange
        _mockOrderManager.Setup(manager => manager.GetOrderSubset(null, 1))
            .Returns(new Response<List<Order>>
            {
                Data = new List<Order>(),
                Message = "Mock Success",
                Successful = true
            });

        // Act
        var result = _orderController.GetAll();
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<List<Order>>>();
        }
    }

    [Fact]
    public void GetAll_WithIncorrectFilter_ShouldReturnBadRequest()
    {
        // Arrange
        
        // Act
        var result = _orderController.GetAll("wrongFilter", 1);
        var badRequestResult = result as BadRequestObjectResult;

        // Assert
        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult?.Value.Should().BeOfType<Response<List<Order>>>();
        }
    }

    [Fact]
    public void GetAll_WithCorrectFilter_ShouldReturnOK()
    {
        // Arrange
        _mockOrderManager.Setup(manager => manager.GetOrderSubset(OrderStatus.PLACED, 1))
            .Returns(new Response<List<Order>>
            {
                Data = new List<Order>(),
                Message = "Mock Success",
                Successful = true
            });
        
        // Act
        var result = _orderController.GetAll("placed");
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<List<Order>>>();
        }
    }

    [Fact]
    public void CreateOrder_WithIncorrectOrderParameters_ShouldReturnBadRequest()
    {
        // Arrange
        var orderToAdd = new PlaceOrderSchema {TableId = -1, TotalPrice = 69.0m};
        _mockOrderManager
            .Setup(manager => manager.AddOrder(It.IsAny<Order>()))
            .Returns(new Response
            {
                Message = "Mock Failure",
                Successful = false
            });

        // Act
        var result = _orderController.CreateOrder(orderToAdd);
        var badRequestResult = result as BadRequestObjectResult;

        // Assert
        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void CreateOrder_WithLongNote_ShouldCutOffNote()
    {
        // Arrange
        var note = "";
        for (var i = 0; i < 2048; i++)
            note += "x";

        var orderToAdd = new PlaceOrderSchema { Note = note };
        _mockOrderManager
            .Setup(manager => manager.AddOrder(It.IsAny<Order>()))
            .Returns(new Response
            {
                Message = "Mock Success",
                Successful = true
            });

        // Act
        var result = _orderController.CreateOrder(orderToAdd);
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<Order>>();
            var value = okResult?.Value as Response<Order>;
            value.Should().NotBeNull();
            value?.Data?.Note.Should().HaveLength(1024);
        }
    }

    [Fact]
    public void CreateOrder_WithCorrectOrderParameters_ShouldReturnOK()
    {
        // Arrange
        var orderToAdd = new PlaceOrderSchema {TableId = 420, TotalPrice = 69.0m, Note = "hoi"};
        _mockOrderManager
            .Setup(manager => manager.AddOrder(It.IsAny<Order>()))
            .Returns(new Response
            {
                Message = "Mock Success",
                Successful = true
            });

        // Act
        var result = _orderController.CreateOrder(orderToAdd);
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void GetOrder_WhenOrderCanBeFound_ShouldReturnOK()
    {
        // Arrange 
        _mockOrderManager
            .Setup(manager => manager.GetOrder(TestId))
            .Returns(new Response<Order>
            {
                Data = new Order(),
                Message = "Mock Success",
                Successful = true
            });

        // Act
        var result = _orderController.GetOrder(TestId);
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void GetOrder_WhenOrderCannotBeFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.GetOrder(TestId))
            .Returns(new Response<Order>
            {
                Data = null,
                Message = "Mock Failure",
                Successful = false
            });

        // Act
        var result = _orderController.GetOrder(TestId);
        var notFoundResult = result as NotFoundObjectResult;
        
        // Assert
        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void DeleteOrder_WhenOrderCannotBeFound_ShouldReturnBadRequest()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.DeleteOrder(TestId))
            .Returns(false);

        // Act
        var result = _orderController.DeleteOrder(TestId);
        var badRequestResult = result as BadRequestObjectResult;

        // Assert
        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult?.Value.Should().BeOfType<Response>();
        }
    }

    [Fact]
    public void DeleteOrder_WhenOrderCanBeDeleted_ShouldReturnOK()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.DeleteOrder(TestId))
            .Returns(true);

        // Act
        var result = _orderController.DeleteOrder(TestId);
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response>();
        }
    }

    [Fact]
    public void AddItems_WhenItemsCanBeAdded_ShouldReturnOK()
    {
        // Arrange 
        _mockOrderManager
            .Setup(manager => manager.AddItemsToOrder(TestId, It.IsAny<OrderItem[]>()))
            .Returns(new Response<Order>
            {
                Data = new Order(),
                Message = "Mock Success",
                Successful = true
            });

        // Act
        var result = _orderController.AddItems(TestId, It.IsAny<OrderItem[]>());
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void AddItems_WhenOrderCannotBeFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.AddItemsToOrder(TestId, It.IsAny<OrderItem[]>()))
            .Returns(new Response<Order>
            {
                Data = null,
                Message = "404: Mocked Failure",
                Successful = false
            });
        
        // Act
        var result = _orderController.AddItems(TestId, It.IsAny<OrderItem[]>());
        var notFoundResult = result as NotFoundObjectResult;

        // Assert
        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void AddItems_WhenItemParametersAreInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.AddItemsToOrder(TestId, It.IsAny<OrderItem[]>()))
            .Returns(new Response<Order>
            {
                Data = null,
                Message = "400: Mocked Failure",
                Successful = false
            });
        
        // Act
        var result = _orderController.AddItems(TestId, It.IsAny<OrderItem[]>());
        var badRequestResult = result as BadRequestObjectResult;

        // Assert
        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void DeleteItem_WhenOrderOrItemCannotBeFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.DeleteItemFromOrder(TestId, It.IsAny<int>()))
            .Returns(new Response<Order>
            {
                Data = null,
                Message = "404: Mocked Failure",
                Successful = false
            });
        
        // Act
        var result = _orderController.DeleteItem(TestId, It.IsAny<int>());
        var notFoundResult = result as NotFoundObjectResult;

        // Assert
        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void DeleteItem_WhenItemCanBeDeleted_ShouldReturnOK()
    {
        // Arrange 
        _mockOrderManager
            .Setup(manager => manager.DeleteItemFromOrder(TestId, It.IsAny<int>()))
            .Returns(new Response<Order>
            {
                Data = new Order(),
                Message = "Mock Success",
                Successful = true
            });

        // Act
        var result = _orderController.DeleteItem(TestId, It.IsAny<int>());
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void UpdateOrder_WhenNewDetailsAreInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.UpdateOrderDetails(TestId, It.IsAny<Order>()))
            .Returns(new Response<Order>
            {
                Data = new Order(),
                Message = "400: Mock Failure",
                Successful = false
            });

        // Act
        var result = _orderController.UpdateOrder(TestId, It.IsAny<Order>());
        var badRequestResult = result as BadRequestObjectResult;

        // Assert
        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }
    
    [Fact]
    public void UpdateOrder_WhenOrderCannotBeFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.UpdateOrderDetails(TestId, It.IsAny<Order>()))
            .Returns(new Response<Order>
            {
                Data = new Order(),
                Message = "404: Mock Failure",
                Successful = false
            });

        // Act
        var result = _orderController.UpdateOrder(TestId, It.IsAny<Order>());
        var notFoundResult = result as NotFoundObjectResult;

        // Assert
        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }
    
    [Fact]
    public void UpdateOrder_WhenOrderCanBeUpdated_ShouldReturnOK()
    {
        // Arrange
        _mockOrderManager
            .Setup(manager => manager.UpdateOrderDetails(TestId, It.IsAny<Order>()))
            .Returns(new Response<Order>
            {
                Data = new Order(),
                Message = "Mock Success",
                Successful = true
            });

        // Act
        var result = _orderController.UpdateOrder(TestId, It.IsAny<Order>());
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<Order>>();
        }
    }

    [Fact]
    public void GetItem_WhenItemCanBeFound_ShouldReturnOK()
    {
        // Arrange 
        _mockOrderManager
            .Setup(manager => manager.GetItemFromOrder(TestId, It.IsAny<int>()))
            .Returns(new Response<OrderItem>()
            {
                Data = new OrderItem(),
                Message = "Mock Success",
                Successful = true
            });

        // Act
        var result = _orderController.GetItem(TestId, It.IsAny<int>());
        var okResult = result as OkObjectResult;

        // Assert
        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult?.Value.Should().BeOfType<Response<OrderItem>>();
        }
    }
    
    [Fact]
    public void GetItem_WhenOrderOrItemCannotBeFound_ShouldNotFound()
    {
        // Arrange 
        _mockOrderManager
            .Setup(manager => manager.GetItemFromOrder(TestId, It.IsAny<int>()))
            .Returns(new Response<OrderItem>()
            {
                Data = null,
                Message = "Mock Failure",
                Successful = false
            });

        // Act
        var result = _orderController.GetItem(TestId, It.IsAny<int>());
        var notFoundResult = result as NotFoundObjectResult;

        // Assert
        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult?.Value.Should().BeOfType<Response<OrderItem>>();
        }
    }
}