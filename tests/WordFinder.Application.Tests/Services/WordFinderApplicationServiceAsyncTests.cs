using Microsoft.Extensions.Logging;
using Moq;
using WordFinder.Application.Common;
using WordFinder.Application.DTOs;
using WordFinder.Application.Services;
using WordFinder.Application.Validators;
using WordFinder.Domain.Factories;
using Xunit;

namespace WordFinder.Application.Tests.Services;

public class WordFinderApplicationServiceAsyncTests
{
    private readonly Mock<ILogger<WordFinderApplicationService>> _mockLogger;
    private readonly Mock<IWordFinderFactory> _mockWordFinderFactory;
    private readonly Mock<IWordSearchRequestValidator> _mockValidator;
    private readonly WordFinderApplicationService _service;

    public WordFinderApplicationServiceAsyncTests()
    {
        _mockLogger = new Mock<ILogger<WordFinderApplicationService>>();
        _mockWordFinderFactory = new Mock<IWordFinderFactory>();
        _mockValidator = new Mock<IWordSearchRequestValidator>();
        _service = new WordFinderApplicationService(_mockLogger.Object, _mockWordFinderFactory.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task SearchWordsAsync_WithCancelledToken_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new WordSearchRequest
        {
            Matrix = new List<string> { "abc", "def" },
            WordStream = new List<string> { "abc" }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var result = await _service.SearchWordsAsync(request, cts.Token);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("cancelled", result.Error.ToLower());
    }

    [Fact]
    public async Task SearchWordsAsync_WithValidRequest_ShouldUseAsyncValidation()
    {
        // Arrange
        var request = new WordSearchRequest
        {
            Matrix = new List<string> { "abc", "def" },
            WordStream = new List<string> { "abc" }
        };

        var validationResult = ValidationResult.Success();
        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(validationResult);

        var mockWordFinder = new Mock<Domain.IWordFinder>();
        mockWordFinder.Setup(w => w.Find(It.IsAny<IEnumerable<string>>()))
                     .Returns(new List<string> { "abc" });

        _mockWordFinderFactory.Setup(f => f.CreateWordFinder(It.IsAny<IEnumerable<string>>()))
                             .Returns(mockWordFinder.Object);

        // Act
        var result = await _service.SearchWordsAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchWordsAsync_WithCancellationDuringValidation_ShouldHandleGracefully()
    {
        // Arrange
        var request = new WordSearchRequest
        {
            Matrix = new List<string> { "abc", "def" },
            WordStream = new List<string> { "abc" }
        };

        var cts = new CancellationTokenSource();
        
        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .Returns(async () => 
                     {
                         await Task.Delay(100, cts.Token); // This will throw OperationCanceledException
                         return ValidationResult.Success();
                     });

        // Act & Assert
        cts.Cancel();
        var result = await _service.SearchWordsAsync(request, cts.Token);
        
        Assert.True(result.IsFailure);
        Assert.Contains("cancelled", result.Error.ToLower());
    }
}
