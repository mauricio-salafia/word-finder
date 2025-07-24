using WordFinder.Domain.Entities;
using WordFinder.Domain.Services;

namespace WordFinder.Domain.Services;

/// <summary>
/// Default implementation of word search algorithm for challenge compliance
/// This is a simple implementation that exists in the domain layer to avoid circular dependencies
/// </summary>
internal class DefaultWordSearchAlgorithm : IWordSearchAlgorithmService
{
    private const int MaxResults = 10;

    public IEnumerable<string> FindWords(CharacterMatrix matrix, IEnumerable<string>? wordStream)
    {
        if (matrix == null || wordStream == null)
            return Enumerable.Empty<string>();
        
        // Remove duplicates and convert to lowercase for case-insensitive comparison
        var uniqueWords = wordStream
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => w.ToLowerInvariant())
            .Distinct()
            .ToHashSet();

        if (!uniqueWords.Any())
            return Enumerable.Empty<string>();

        var foundWords = new Dictionary<string, WordOccurrence>();

        // Search horizontally
        SearchHorizontally(matrix, uniqueWords, foundWords);

        // Search vertically
        SearchVertically(matrix, uniqueWords, foundWords);

        // Return top 10 most repeated words
        return foundWords.Values
            .OrderByDescending(w => w.Count)
            .ThenBy(w => w.Word) // Secondary sort for consistency
            .Take(MaxResults)
            .Select(w => w.Word);
    }

    private void SearchHorizontally(CharacterMatrix matrix, HashSet<string> wordsToFind, Dictionary<string, WordOccurrence> foundWords)
    {
        for (int row = 0; row < matrix.Rows; row++)
        {
            // Get the entire row as a string for efficient searching
            var rowString = GetRowString(matrix, row);
            
            // Check all possible substrings
            SearchInString(rowString, wordsToFind, foundWords);
        }
    }

    private void SearchVertically(CharacterMatrix matrix, HashSet<string> wordsToFind, Dictionary<string, WordOccurrence> foundWords)
    {
        for (int col = 0; col < matrix.Columns; col++)
        {
            // Get the entire column as a string for efficient searching
            var colString = GetColumnString(matrix, col);
            
            // Check all possible substrings
            SearchInString(colString, wordsToFind, foundWords);
        }
    }

    private void SearchInString(string text, HashSet<string> wordsToFind, Dictionary<string, WordOccurrence> foundWords)
    {
        foreach (var word in wordsToFind)
        {
            int index = 0;
            while ((index = text.IndexOf(word, index, StringComparison.Ordinal)) != -1)
            {
                if (foundWords.TryGetValue(word, out var occurrence))
                {
                    occurrence.IncrementCount();
                }
                else
                {
                    foundWords[word] = new WordOccurrence(word);
                }
                index++; // Move to next position to find overlapping occurrences
            }
        }
    }

    private string GetRowString(CharacterMatrix matrix, int row)
    {
        var chars = new char[matrix.Columns];
        for (int col = 0; col < matrix.Columns; col++)
        {
            chars[col] = matrix.GetCharacter(row, col);
        }
        return new string(chars);
    }

    private string GetColumnString(CharacterMatrix matrix, int col)
    {
        var chars = new char[matrix.Rows];
        for (int row = 0; row < matrix.Rows; row++)
        {
            chars[row] = matrix.GetCharacter(row, col);
        }
        return new string(chars);
    }
}
