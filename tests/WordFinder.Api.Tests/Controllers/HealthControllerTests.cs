using Microsoft.AspNetCore.Mvc;
using WordFinder.Api.Controllers;
using Xunit;

namespace WordFinder.Api.Tests.Controllers;

public class HealthControllerTests
{
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _controller = new HealthController();
    }

    [Fact]
    public void Health_ShouldReturnOkWithHealthStatus()
    {
        // Act
        var result = _controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var healthStatus = okResult.Value;
        Assert.NotNull(healthStatus);
        
        // Verify the response has the expected structure
        var properties = healthStatus!.GetType().GetProperties();
        Assert.Contains(properties, p => p.Name == "Status");
        Assert.Contains(properties, p => p.Name == "Timestamp");
    }
}
