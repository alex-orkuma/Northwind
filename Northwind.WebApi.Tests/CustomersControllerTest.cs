using Northwind.WebApi;
using Xunit;
using Moq;
using Northwind.WebApi.Repositories;

namespace Northwind.WebApi.Tests;

using Microsoft.AspNetCore.Mvc;
using Northwind.WebApi.Controllers;
using Packt.Shared;

public class CustomersControllerTest
{
    [Fact]
    public async Task GetCustomerAsyncReturnsCustomerWithSameId()
    {
        // ARRANGE
        var mockRepository = new Mock<ICustomerRepository>();
        mockRepository.Setup(x => x.RetrieveAsync("MXNT")).
            Returns(Task.FromResult(new Customer { CustomerId = "MXNT" }));

        var customerController = new CustomersController(mockRepository.Object);


        // ACT

        IActionResult customer = await customerController.GetCustomer("MXNT").ConfigureAwait(false);

        // ASSERT
        Assert.IsType<OkObjectResult>(customer as OkObjectResult);
    }

    [Fact]
    public async Task GetCustomerAsyncReturnsNotFound()
    {
        // ARRANGE
        var mockrepositoyr = new Mock<ICustomerRepository>();
        var customerController = new CustomersController(mockrepositoyr.Object);

        // ACT
        IActionResult noCustomer = await customerController.GetCustomer("");

        // ASSERT
        Assert.IsType<NotFoundResult>(noCustomer);

       

      
    }
}