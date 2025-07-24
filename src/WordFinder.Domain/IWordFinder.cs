namespace WordFinder.Domain;

/// <summary>
/// Interface for the main WordFinder functionality
/// </summary>
public interface IWordFinder
{
    /// <summary>
    /// Finds words from the word stream in the matrix
    /// </summary>
    /// <param name="wordstream">Stream of words to search for</param>
    /// <returns>Top 10 most repeated words found in the matrix</returns>
    IEnumerable<string> Find(IEnumerable<string>? wordstream);
}
