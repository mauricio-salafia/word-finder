namespace WordFinder.Application.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; private set; }
    public string Error { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
        Errors = new List<string> { error };
    }

    private Result(List<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
        Error = string.Join(", ", errors);
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public static Result<T> Failure(string error) => new(error);

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result<T> Failure(List<string> errors) => new(errors);

    /// <summary>
    /// Implicitly converts a value to a successful result
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Represents the result of an operation without a return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();

    private Result(bool isSuccess, string error = "")
    {
        IsSuccess = isSuccess;
        Error = error;
        if (!string.IsNullOrEmpty(error))
        {
            Errors = new List<string> { error };
        }
    }

    private Result(List<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
        Error = string.Join(", ", errors);
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with a single error
    /// </summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result Failure(List<string> errors) => new(errors);
}
