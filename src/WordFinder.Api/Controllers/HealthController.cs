using Microsoft.AspNetCore.Mvc;

namespace WordFinder.Api.Controllers;

/// <summary>
/// API Controller for health check operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>API health status</returns>
    /// <response code="200">Returns the health status of the API</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
