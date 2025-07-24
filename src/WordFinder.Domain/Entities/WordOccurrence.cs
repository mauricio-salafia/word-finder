namespace WordFinder.Domain.Entities;

/// <summary>
/// Represents a word found in the matrix with its occurrence count
/// </summary>
public class WordOccurrence
{
    public string Word { get; }
    public int Count { get; private set; }

    public WordOccurrence(string word)
    {
        Word = word ?? throw new ArgumentNullException(nameof(word));
        Count = 1;
    }

    public void IncrementCount()
    {
        Count++;
    }

    public override bool Equals(object? obj)
    {
        if (obj is WordOccurrence other)
        {
            return string.Equals(Word, other.Word, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Word.ToLowerInvariant().GetHashCode();
    }
}
