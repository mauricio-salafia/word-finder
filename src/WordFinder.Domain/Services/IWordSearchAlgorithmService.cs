using WordFinder.Domain.Entities;

namespace WordFinder.Domain.Services;

/// <summary>
/// Domain service interface for word search algorithm
/// The interface stays in Domain, implementation will be in Application layer
/// </summary>
public interface IWordSearchAlgorithmService
{
    /// <summary>
    /// Searches for words in the matrix and returns the top 10 most repeated words
    /// </summary>
    /// <param name="matrix">The character matrix to search in</param>
    /// <param name="wordStream">Stream of words to search for</param>
    /// <returns>Top 10 most repeated words found in the matrix</returns>
    IEnumerable<string> FindWords(CharacterMatrix matrix, IEnumerable<string>? wordStream);
}
