namespace ExpenseFlow.Shared.Application.Models;

/// <summary>
/// Discriminated union result type — represents either success or failure.
/// Avoids throwing exceptions for expected business failures (e.g. "expense not found").
///
/// Usage:
///   var result = Result<ExpenseDto>.Success(dto);
///   var result = Result<ExpenseDto>.Failure("Expense not found");
///
///   if (result.IsSuccess) { use result.Value }
///   else { return 404 with result.Error }
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string Error { get; }

    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default, error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error);
}
