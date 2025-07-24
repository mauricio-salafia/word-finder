using WordFinder.Domain.Entities;
using Xunit;

namespace WordFinder.Domain.Tests.Entities;

public class CharacterMatrixTests
{
    [Fact]
    public void Constructor_WithValidMatrix_ShouldCreateMatrix()
    {
        // Arrange
        var matrix = new[] { "abc", "def", "ghi" };

        // Act
        var characterMatrix = new CharacterMatrix(matrix);

        // Assert
        Assert.Equal(3, characterMatrix.Rows);
        Assert.Equal(3, characterMatrix.Columns);
    }

    [Fact]
    public void Constructor_WithNullMatrix_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CharacterMatrix(null!));
    }

    [Fact]
    public void Constructor_WithEmptyMatrix_ShouldThrowArgumentException()
    {
        // Arrange
        var matrix = Array.Empty<string>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CharacterMatrix(matrix));
    }

    [Fact]
    public void Constructor_WithMatrixTooLarge_ShouldThrowArgumentException()
    {
        // Arrange
        var matrix = new string[65]; // Exceeds 64x64 limit
        for (int i = 0; i < 65; i++)
        {
            matrix[i] = "a";
        }

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CharacterMatrix(matrix));
    }

    [Fact]
    public void Constructor_WithInconsistentRowLengths_ShouldThrowArgumentException()
    {
        // Arrange
        var matrix = new[] { "abc", "de", "fgh" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CharacterMatrix(matrix));
    }

    [Fact]
    public void GetCharacter_WithValidCoordinates_ShouldReturnCorrectCharacter()
    {
        // Arrange
        var matrix = new[] { "ABC", "DEF", "GHI" };
        var characterMatrix = new CharacterMatrix(matrix);

        // Act & Assert
        Assert.Equal('a', characterMatrix.GetCharacter(0, 0)); // Case insensitive
        Assert.Equal('e', characterMatrix.GetCharacter(1, 1));
        Assert.Equal('i', characterMatrix.GetCharacter(2, 2));
    }

    [Fact]
    public void GetCharacter_WithInvalidCoordinates_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var matrix = new[] { "abc", "def", "ghi" };
        var characterMatrix = new CharacterMatrix(matrix);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => characterMatrix.GetCharacter(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => characterMatrix.GetCharacter(0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => characterMatrix.GetCharacter(3, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => characterMatrix.GetCharacter(0, 3));
    }

    [Fact]
    public void GetHorizontalWord_WithValidParameters_ShouldReturnCorrectWord()
    {
        // Arrange
        var matrix = new[] { "abcd", "efgh", "ijkl" };
        var characterMatrix = new CharacterMatrix(matrix);

        // Act
        var word = characterMatrix.GetHorizontalWord(0, 1, 3);

        // Assert
        Assert.Equal("bcd", word);
    }

    [Fact]
    public void GetHorizontalWord_WithInvalidParameters_ShouldReturnEmptyString()
    {
        // Arrange
        var matrix = new[] { "abcd", "efgh", "ijkl" };
        var characterMatrix = new CharacterMatrix(matrix);

        // Act & Assert
        Assert.Equal(string.Empty, characterMatrix.GetHorizontalWord(-1, 0, 2));
        Assert.Equal(string.Empty, characterMatrix.GetHorizontalWord(0, 3, 2)); // Would exceed bounds
    }

    [Fact]
    public void GetVerticalWord_WithValidParameters_ShouldReturnCorrectWord()
    {
        // Arrange
        var matrix = new[] { "abcd", "efgh", "ijkl" };
        var characterMatrix = new CharacterMatrix(matrix);

        // Act
        var word = characterMatrix.GetVerticalWord(0, 1, 3);

        // Assert
        Assert.Equal("bfj", word);
    }

    [Fact]
    public void GetVerticalWord_WithInvalidParameters_ShouldReturnEmptyString()
    {
        // Arrange
        var matrix = new[] { "abcd", "efgh", "ijkl" };
        var characterMatrix = new CharacterMatrix(matrix);

        // Act & Assert
        Assert.Equal(string.Empty, characterMatrix.GetVerticalWord(0, -1, 2));
        Assert.Equal(string.Empty, characterMatrix.GetVerticalWord(2, 0, 2)); // Would exceed bounds
    }
}
