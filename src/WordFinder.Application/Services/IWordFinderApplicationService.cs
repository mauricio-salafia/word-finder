using WordFinder.Application.Common;
using WordFinder.Application.DTOs;

namespace WordFinder.Application.Services;

/// <summary>
/// Application service interface for word finding operations
/// </summary>
public interface IWordFinderApplicationService
{
    /// <summary>
    /// Searches for words in a matrix
    /// </summary>
    /// <param name="request">The search request containing matrix and word stream</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Search results with found words and metrics</returns>
    Task<Result<WordSearchResponse>> SearchWordsAsync(WordSearchRequest request, CancellationToken cancellationToken = default);
}
