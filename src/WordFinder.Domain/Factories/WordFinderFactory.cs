using WordFinder.Domain.Entities;
using WordFinder.Domain.Services;

namespace WordFinder.Domain.Factories;

/// <summary>
/// Factory for creating WordFinder instances with proper dependency injection
/// </summary>
public interface IWordFinderFactory
{
    /// <summary>
    /// Creates a WordFinder instance from a matrix
    /// </summary>
    /// <param name="matrix">The character matrix</param>
    /// <returns>WordFinder instance</returns>
    IWordFinder CreateWordFinder(IEnumerable<string> matrix);
}

/// <summary>
/// Implementation of WordFinder factory
/// </summary>
public class WordFinderFactory : IWordFinderFactory
{
    private readonly IWordSearchAlgorithmService _algorithmService;

    public WordFinderFactory(IWordSearchAlgorithmService algorithmService)
    {
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
    }

    public IWordFinder CreateWordFinder(IEnumerable<string> matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        var characterMatrix = new CharacterMatrix(matrix);
        
        // Always use the algorithm service - it has ownership of word finding logic
        return new WordFinder(characterMatrix, _algorithmService);
    }
}
