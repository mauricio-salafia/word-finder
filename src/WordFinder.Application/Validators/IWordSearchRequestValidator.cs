using WordFinder.Application.DTOs;

namespace WordFinder.Application.Validators;

/// <summary>
/// Interface for validating word search requests
/// </summary>
public interface IWordSearchRequestValidator
{
    /// <summary>
    /// Validates a word search request asynchronously
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Validation result indicating success or failure with errors</returns>
    Task<ValidationResult> ValidateAsync(WordSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a word search request synchronously (legacy support)
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <returns>Validation result indicating success or failure with errors</returns>
    ValidationResult Validate(WordSearchRequest request);
}
