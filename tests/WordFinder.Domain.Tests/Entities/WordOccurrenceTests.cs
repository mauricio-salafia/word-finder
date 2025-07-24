using WordFinder.Domain.Entities;
using Xunit;

namespace WordFinder.Domain.Tests.Entities;

public class WordOccurrenceTests
{
    [Fact]
    public void Constructor_WithValidWord_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var wordOccurrence = new WordOccurrence("test");

        // Assert
        Assert.Equal("test", wordOccurrence.Word);
        Assert.Equal(1, wordOccurrence.Count);
    }

    [Fact]
    public void Constructor_WithNullWord_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WordOccurrence(null!));
    }

    [Fact]
    public void IncrementCount_ShouldIncreaseCountByOne()
    {
        // Arrange
        var wordOccurrence = new WordOccurrence("test");

        // Act
        wordOccurrence.IncrementCount();
        wordOccurrence.IncrementCount();

        // Assert
        Assert.Equal(3, wordOccurrence.Count);
    }

    [Fact]
    public void Equals_WithSameWordDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        var word1 = new WordOccurrence("Test");
        var word2 = new WordOccurrence("test");

        // Act & Assert
        Assert.True(word1.Equals(word2));
        Assert.True(word2.Equals(word1));
    }

    [Fact]
    public void Equals_WithDifferentWords_ShouldReturnFalse()
    {
        // Arrange
        var word1 = new WordOccurrence("test");
        var word2 = new WordOccurrence("other");

        // Act & Assert
        Assert.False(word1.Equals(word2));
        Assert.False(word2.Equals(word1));
    }

    [Fact]
    public void GetHashCode_WithSameWordDifferentCase_ShouldReturnSameHashCode()
    {
        // Arrange
        var word1 = new WordOccurrence("Test");
        var word2 = new WordOccurrence("test");

        // Act & Assert
        Assert.Equal(word1.GetHashCode(), word2.GetHashCode());
    }
}
