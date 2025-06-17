using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using InferenceLlvm.Controllers;
using InferenceLlvm;

namespace InferenceLlvm.Tests;

public class HealthCheckTests
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        var mockService = new Mock<IRerankerService>();
        mockService.Setup(s => s.GetHealthAsync())
                   .ReturnsAsync(new HealthResponse { Status = "healthy", ModelLoaded = true });

        var logger = Mock.Of<ILogger<RerankerController>>();
        var controller = new RerankerController(mockService.Object, logger);

        var result = await controller.Health();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<HealthResponse>(ok.Value);
        Assert.True(response.ModelLoaded);
    }
}
