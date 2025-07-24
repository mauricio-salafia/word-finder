namespace WordFinder.Application.DTOs;

/// <summary>
/// Response DTO for word search operations
/// </summary>
public class WordSearchResponse
{
    /// <summary>
    /// List of words found in the matrix, ordered by frequency (most frequent first), then alphabetically. Maximum 10 words.
    /// </summary>
    /// <example>["chill", "cold", "wind"]</example>
    public List<string> FoundWords { get; set; } = new();

    /// <summary>
    /// Total number of unique words that were searched for in the matrix.
    /// </summary>
    /// <example>4</example>
    public int TotalWordsSearched { get; set; }

    /// <summary>
    /// Actual processing time in milliseconds for the search operation.
    /// </summary>
    /// <example>15</example>
    public int ProcessingTimeMs { get; set; }
}
