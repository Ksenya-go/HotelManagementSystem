namespace HotelManagementSystem.Application.Common.Cqrs.Results;

public sealed class Result<T>
{
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string code, string message) => new(new Error(code, message));
}

