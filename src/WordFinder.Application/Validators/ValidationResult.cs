using WordFinder.Application.Common;

namespace WordFinder.Application.Validators;

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private ValidationResult(bool isValid, List<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new(true, new List<string>());

    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    public static ValidationResult Failure(List<string> errors) => new(false, errors);

    /// <summary>
    /// Creates a failed validation result with a single error
    /// </summary>
    public static ValidationResult Failure(string error) => new(false, new List<string> { error });
}
