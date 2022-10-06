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
using Xunit;

namespace Unit_testing;

public class OrderControllerTests
{
    private readonly Mock<IOrderManager> _mockOrderManager;
    private readonly OrderController _orderController;

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
}