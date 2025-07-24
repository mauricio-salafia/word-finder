using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WordFinder.Api.Controllers;
using WordFinder.Application.Common;
using WordFinder.Application.DTOs;
using WordFinder.Application.Services;
using Xunit;

namespace WordFinder.Api.Tests.Controllers;

public class WordFinderControllerTests
{
    private readonly Mock<IWordFinderApplicationService> _mockWordFinderService;
    private readonly Mock<ILogger<WordFinderController>> _mockLogger;
    private readonly WordFinderController _controller;

    public WordFinderControllerTests()
    {
        _mockWordFinderService = new Mock<IWordFinderApplicationService>();
        _mockLogger = new Mock<ILogger<WordFinderController>>();
        _controller = new WordFinderController(_mockWordFinderService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new WordFinderController(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new WordFinderController(_mockWordFinderService.Object, null!));
    }

    [Fact]
    public async Task SearchWords_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = new WordSearchRequest
        {
            Matrix = new List<string> { "abc", "def", "ghi" },
            WordStream = new List<string> { "abc", "def" }
        };

        var expectedResponse = new WordSearchResponse
        {
            FoundWords = new List<string> { "abc", "def" },
            TotalWordsSearched = 2,
            ProcessingTimeMs = 10
        };

        _mockWordFinderService.Setup(s => s.SearchWordsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WordSearchResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.SearchWords(request);

        // Assert
        var okResult = Assert.IsType<ActionResult<WordSearchResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<WordSearchResponse>(objectResult.Value);
        
        Assert.Equal(expectedResponse.FoundWords, response.FoundWords);
        Assert.Equal(expectedResponse.TotalWordsSearched, response.TotalWordsSearched);
        Assert.Equal(expectedResponse.ProcessingTimeMs, response.ProcessingTimeMs);
    }

    [Fact]
    public async Task SearchWords_WithNullRequest_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.SearchWords(null!);

        // Assert
        var actionResult = Assert.IsType<ActionResult<WordSearchResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Request cannot be null", badRequestResult.Value);
    }

    [Fact]
    public async Task SearchWords_WithServiceThrowingArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new WordSearchRequest();
        var exceptionMessage = "Invalid matrix";

        _mockWordFinderService.Setup(s => s.SearchWordsAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException(exceptionMessage));

        // Act
        var result = await _controller.SearchWords(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<WordSearchResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(exceptionMessage, badRequestResult.Value);
    }

    [Fact]
    public async Task SearchWords_WithServiceThrowingGenericException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new WordSearchRequest();

        _mockWordFinderService.Setup(s => s.SearchWordsAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.SearchWords(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<WordSearchResponse>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An internal server error occurred", statusCodeResult.Value);
    }

    [Fact]
    public async Task SearchWords_ShouldLogInformation()
    {
        // Arrange
        var request = new WordSearchRequest
        {
            Matrix = new List<string> { "abc" },
            WordStream = new List<string> { "abc" }
        };

        var response = new WordSearchResponse();
        _mockWordFinderService.Setup(s => s.SearchWordsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WordSearchResponse>.Success(response));

        // Act
        await _controller.SearchWords(request);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received word search request")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Word search request completed successfully")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
