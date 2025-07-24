using Xunit;

namespace WordFinder.Domain.Tests;

public class WordFinderTests
{
    private readonly string[] _testMatrix = new[]
    {
        "abcdc",
        "fgwio", 
        "chill",
        "pqnsd",
        "uvdxy"
    };

    [Fact]
    public void Constructor_WithValidMatrix_ShouldCreateInstance()
    {
        // Act
        var wordFinder = new Domain.WordFinder(_testMatrix);

        // Assert
        Assert.NotNull(wordFinder);
    }

    [Fact]
    public void Constructor_WithNullMatrix_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Domain.WordFinder(null!));
    }

    [Fact]
    public void Find_WithValidWordStream_ShouldReturnFoundWords()
    {
        // Arrange
        var wordFinder = new Domain.WordFinder(_testMatrix);
        var wordStream = new[] { "chill", "cold", "wind", "abc", "fgw" };

        // Act
        var result = wordFinder.Find(wordStream).ToList();

        // Assert
        Assert.Contains("chill", result);
        Assert.Contains("abc", result);
        Assert.Contains("fgw", result);
        Assert.Contains("cold", result); // "cold" is found vertically in column 4
        Assert.Contains("wind", result); // "wind" is found vertically in column 2
    }

    [Fact]
    public void Find_WithNullWordStream_ShouldReturnEmptyResult()
    {
        // Arrange
        var wordFinder = new Domain.WordFinder(_testMatrix);

        // Act
        var result = wordFinder.Find(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Find_WithEmptyWordStream_ShouldReturnEmptyResult()
    {
        // Arrange
        var wordFinder = new Domain.WordFinder(_testMatrix);

        // Act
        var result = wordFinder.Find(Array.Empty<string>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Find_WithExampleFromChallenge_ShouldWork()
    {
        // Arrange - Based on the challenge description: matrix contains "chill", "cold" and "wind"
        var matrix = new[]
        {
            "abcdc",
            "fgwio",
            "chill",
            "pqnsd",
            "uvdxy"
        };
        var wordFinder = new Domain.WordFinder(matrix);
        var wordStream = new[] { "chill", "cold", "wind", "snow" };

        // Act
        var result = wordFinder.Find(wordStream).ToList();

        // Assert
        Assert.Contains("chill", result); // horizontal in row 2
        Assert.Contains("cold", result); // vertical in column 4
        Assert.Contains("wind", result); // vertical in column 2
        Assert.DoesNotContain("snow", result);
    }

    [Fact]
    public void Find_ShouldReturnAtMost10Words()
    {
        // Arrange
        var largeMatrix = GenerateLargeMatrix();
        var wordFinder = new Domain.WordFinder(largeMatrix);
        var wordStream = GenerateWordStream(20); // More than 10 words

        // Act
        var result = wordFinder.Find(wordStream).ToList();

        // Assert
        Assert.True(result.Count <= 10);
    }

    private static string[] GenerateLargeMatrix()
    {
        var matrix = new string[10];
        for (int i = 0; i < 10; i++)
        {
            matrix[i] = "abcdefghij";
        }
        return matrix;
    }

    private static string[] GenerateWordStream(int count)
    {
        var words = new string[count];
        for (int i = 0; i < count; i++)
        {
            words[i] = ((char)('a' + i)).ToString();
        }
        return words;
    }
}
