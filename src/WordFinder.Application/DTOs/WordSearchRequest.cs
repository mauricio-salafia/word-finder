using System.ComponentModel.DataAnnotations;

namespace WordFinder.Application.DTOs;

/// <summary>
/// Request DTO for word search operations
/// </summary>
public class WordSearchRequest
{
    /// <summary>
    /// Character matrix where words will be searched. Maximum size 64x64, all rows must have same length.
    /// </summary>
    /// <example>["abcdc", "fgwio", "chill", "pqnsd", "uvdxy"]</example>
    [Required]
    public List<string> Matrix { get; set; } = new();

    /// <summary>
    /// Stream of words to search for in the matrix. Searches are case-insensitive.
    /// </summary>
    /// <example>["chill", "cold", "wind", "snow"]</example>
    [Required]
    public List<string> WordStream { get; set; } = new();
}
