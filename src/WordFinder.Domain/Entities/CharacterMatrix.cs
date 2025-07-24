using System.Text;

namespace WordFinder.Domain.Entities;

/// <summary>
/// Represents a character matrix with efficient search capabilities
/// </summary>
public class CharacterMatrix
{
    private readonly char[,] _matrix;
    private readonly int _rows;
    private readonly int _columns;

    public CharacterMatrix(IEnumerable<string> matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        var matrixArray = matrix.ToArray();
        
        if (matrixArray.Length == 0)
            throw new ArgumentException("Matrix cannot be empty", nameof(matrix));

        ValidateMatrix(matrixArray);

        _rows = matrixArray.Length;
        _columns = matrixArray[0].Length;
        _matrix = new char[_rows, _columns];

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                _matrix[i, j] = char.ToLowerInvariant(matrixArray[i][j]);
            }
        }
    }

    public int Rows => _rows;
    public int Columns => _columns;

    public char GetCharacter(int row, int column)
    {
        if (row < 0 || row >= _rows || column < 0 || column >= _columns)
            throw new ArgumentOutOfRangeException();

        return _matrix[row, column];
    }

    /// <summary>
    /// Gets all horizontal words starting from a specific position
    /// </summary>
    public string GetHorizontalWord(int row, int startColumn, int length)
    {
        if (row < 0 || row >= _rows || startColumn < 0 || startColumn + length > _columns)
            return string.Empty;

        var sb = new StringBuilder(length);
        for (int j = startColumn; j < startColumn + length; j++)
        {
            sb.Append(_matrix[row, j]);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Gets all vertical words starting from a specific position
    /// </summary>
    public string GetVerticalWord(int startRow, int column, int length)
    {
        if (column < 0 || column >= _columns || startRow < 0 || startRow + length > _rows)
            return string.Empty;

        var sb = new StringBuilder(length);
        for (int i = startRow; i < startRow + length; i++)
        {
            sb.Append(_matrix[i, column]);
        }
        return sb.ToString();
    }

    private static void ValidateMatrix(string[] matrix)
    {
        if (matrix.Length > 64)
            throw new ArgumentException("Matrix size cannot exceed 64x64", nameof(matrix));

        var expectedLength = matrix[0].Length;
        if (expectedLength > 64)
            throw new ArgumentException("Matrix size cannot exceed 64x64", nameof(matrix));

        foreach (var row in matrix)
        {
            if (string.IsNullOrEmpty(row))
                throw new ArgumentException("Matrix rows cannot be null or empty", nameof(matrix));

            if (row.Length != expectedLength)
                throw new ArgumentException("All matrix rows must have the same length", nameof(matrix));
        }
    }

}
