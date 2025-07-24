using WordFinder.Domain.Entities;
using WordFinder.Domain.Services;

namespace WordFinder.Domain;

/// <summary>
/// Main WordFinder class that implements the required interface
/// </summary>
public class WordFinder : IWordFinder
{
    private readonly CharacterMatrix _matrix;
    private readonly IWordSearchAlgorithmService _algorithmService;

    public WordFinder(IEnumerable<string> matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        _matrix = new CharacterMatrix(matrix);
        // For challenge compliance, use a default algorithm implementation
        _algorithmService = new DefaultWordSearchAlgorithm();
    }

    // Internal constructor for dependency injection (used by factory)
    internal WordFinder(CharacterMatrix matrix, IWordSearchAlgorithmService algorithmService)
    {
        _matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
        _algorithmService = algorithmService ?? throw new ArgumentNullException(nameof(algorithmService));
    }

    /// <summary>
    /// Finds words from the word stream in the matrix
    /// </summary>
    /// <param name="wordstream">Stream of words to search for</param>
    /// <returns>Top 10 most repeated words found in the matrix</returns>
    public IEnumerable<string> Find(IEnumerable<string>? wordstream)
    {
        // Always use the algorithm service - it has ownership of the word finding logic
        return _algorithmService.FindWords(_matrix, wordstream);
    }
}
