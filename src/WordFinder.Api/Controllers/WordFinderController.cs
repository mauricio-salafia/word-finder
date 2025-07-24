using Microsoft.AspNetCore.Mvc;
using WordFinder.Application.Common;
using WordFinder.Application.DTOs;
using WordFinder.Application.Services;

namespace WordFinder.Api.Controllers;

/// <summary>
/// API Controller for word finding operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WordFinderController : ControllerBase
{
    private readonly IWordFinderApplicationService _wordFinderService;
    private readonly ILogger<WordFinderController> _logger;

    public WordFinderController(
        IWordFinderApplicationService wordFinderService,
        ILogger<WordFinderController> logger)
    {
        _wordFinderService = wordFinderService ?? throw new ArgumentNullException(nameof(wordFinderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches for words in a character matrix
    /// </summary>
    /// <param name="request">The search request containing the matrix and word stream</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Search results with found words and performance metrics</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/wordfinder/search
    ///     {
    ///       "matrix": [
    ///         "abcdc",
    ///         "fgwio", 
    ///         "chill",
    ///         "pqnsd",
    ///         "uvdxy"
    ///       ],
    ///       "wordStream": ["chill", "cold", "wind", "snow"]
    ///     }
    /// 
    /// Expected response:
    /// 
    ///     {
    ///       "foundWords": ["chill", "cold", "wind"],
    ///       "totalWordsSearched": 4,
    ///       "processingTimeMs": 15
    ///     }
    /// 
    /// The algorithm searches for words horizontally (left to right) and vertically (top to bottom).
    /// In this example:
    /// - "chill" is found horizontally in row 3
    /// - "cold" is found vertically in column 5 (c-o-l-d)
    /// - "wind" is found vertically in column 3 (w-i-n-d)
    /// - "snow" is not found in the matrix
    /// 
    /// Matrix constraints:
    /// - Maximum size: 64x64 characters
    /// - All rows must have the same length
    /// - Case-insensitive search
    /// - Returns top 10 most frequent words found
    /// </remarks>
    /// <response code="200">Returns the search results</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="499">If the request was cancelled by the client</response>
    /// <response code="500">If an internal server error occurs</response>
    [HttpPost("search")]
    [ProducesResponseType(typeof(WordSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), 499)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WordSearchResponse>> SearchWords([FromBody] WordSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received word search request");
            
            if (request == null)
            {
                _logger.LogWarning("Received null request");
                return BadRequest("Request cannot be null");
            }

            var result = await _wordFinderService.SearchWordsAsync(request, cancellationToken);
            
            if (result.IsFailure)
            {
                _logger.LogWarning("Request validation failed with {ErrorCount} errors: {Errors}", 
                    result.Errors.Count, string.Join(", ", result.Errors));
                return BadRequest(new { Errors = result.Errors, Message = result.Error });
            }
            
            _logger.LogInformation("Word search request completed successfully");
            return Ok(result.Value);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Word search request was cancelled");
            return StatusCode(499, "Request was cancelled");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request received");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the word search request");
            return StatusCode(500, "An internal server error occurred");
        }
    }
}
