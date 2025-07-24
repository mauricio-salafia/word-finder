using Microsoft.Extensions.Logging;
using WordFinder.Application.DTOs;

namespace WordFinder.Application.Validators;

/// <summary>
/// Validator for word search requests
/// </summary>
public class WordSearchRequestValidator : IWordSearchRequestValidator
{
    private const int MaxMatrixSize = 64;
    private readonly ILogger<WordSearchRequestValidator> _logger;

    public WordSearchRequestValidator(ILogger<WordSearchRequestValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ValidationResult> ValidateAsync(WordSearchRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            const string error = "Request cannot be null";
            _logger.LogWarning("Validation failed: {Error}", error);
            return ValidationResult.Failure(error);
        }

        var errors = new List<string>();

        // Validate matrix asynchronously
        await ValidateMatrixAsync(request.Matrix, errors, cancellationToken);
        
        // Validate word stream asynchronously  
        await ValidateWordStreamAsync(request.WordStream, errors, cancellationToken);

        if (errors.Count > 0)
        {
            _logger.LogWarning("Request validation failed with {ErrorCount} errors: {Errors}", 
                errors.Count, string.Join(", ", errors));
            return ValidationResult.Failure(errors);
        }

        _logger.LogDebug("Request validation passed");
        return ValidationResult.Success();
    }

    public ValidationResult Validate(WordSearchRequest request)
    {
        return ValidateAsync(request, CancellationToken.None).GetAwaiter().GetResult();
    }

    private async Task ValidateMatrixAsync(List<string> matrix, List<string> errors, CancellationToken cancellationToken)
    {
        await Task.Yield(); // Make it truly async
        cancellationToken.ThrowIfCancellationRequested();

        if (matrix is null || matrix.Count == 0)
        {
            errors.Add("Matrix cannot be null or empty");
            return;
        }

        if (matrix.Count > MaxMatrixSize)
        {
            errors.Add($"Matrix cannot have more than {MaxMatrixSize} rows");
        }

        var firstRowLength = matrix.First().Length;
        if (firstRowLength > MaxMatrixSize)
        {
            errors.Add($"Matrix rows cannot have more than {MaxMatrixSize} characters");
        }

        // Check row consistency asynchronously for large matrices
        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (matrix.Any(row => row.Length != firstRowLength))
            {
                errors.Add("All matrix rows must have the same length");
            }
        }, cancellationToken);
    }

    private async Task ValidateWordStreamAsync(List<string> wordStream, List<string> errors, CancellationToken cancellationToken)
    {
        await Task.Yield(); // Make it truly async
        cancellationToken.ThrowIfCancellationRequested();

        if (wordStream is null)
        {
            errors.Add("WordStream cannot be null");
        }
    }
}
