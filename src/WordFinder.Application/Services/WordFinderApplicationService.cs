using Microsoft.Extensions.Logging;
using WordFinder.Application.Common;
using WordFinder.Application.DTOs;
using WordFinder.Application.Services;
using WordFinder.Application.Validators;
using WordFinder.Domain.Factories;

namespace WordFinder.Application.Services;

/// <summary>
/// Application service for word finding operations with logging and metrics
/// </summary>
public class WordFinderApplicationService : IWordFinderApplicationService
{
    private readonly ILogger<WordFinderApplicationService> _logger;
    private readonly IWordFinderFactory _wordFinderFactory;
    private readonly IWordSearchRequestValidator _validator;

    public WordFinderApplicationService(
        ILogger<WordFinderApplicationService> logger,
        IWordFinderFactory wordFinderFactory,
        IWordSearchRequestValidator validator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _wordFinderFactory = wordFinderFactory ?? throw new ArgumentNullException(nameof(wordFinderFactory));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<Result<WordSearchResponse>> SearchWordsAsync(WordSearchRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            _logger.LogError("Request cannot be null");
            return Result<WordSearchResponse>.Failure("Request cannot be null");
        }

        // Validate the request using the validator asynchronously
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Invalid request: {ErrorMessage}", string.Join(", ", validationResult.Errors));
            return Result<WordSearchResponse>.Failure(validationResult.Errors);
        }

        try
        {
            _logger.LogInformation("Searching for words in matrix");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Create word finder and search asynchronously
            var wordFinder = await Task.Run(() => _wordFinderFactory.CreateWordFinder(request.Matrix), cancellationToken);
            var foundWords = await Task.Run(() => wordFinder.Find(request.WordStream), cancellationToken);
            
            stopwatch.Stop();
            
            _logger.LogInformation("Found {Count} words in {ElapsedMs}ms", foundWords.Count(), stopwatch.ElapsedMilliseconds);
            
            var response = new WordSearchResponse
            {
                FoundWords = foundWords.ToList(),
                TotalWordsSearched = request.WordStream.Count,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
            
            return Result<WordSearchResponse>.Success(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Word search operation was cancelled");
            return Result<WordSearchResponse>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for words");
            return Result<WordSearchResponse>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
