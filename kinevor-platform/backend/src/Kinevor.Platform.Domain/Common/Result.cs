namespace Kinevor.Platform.Domain.Common;

/// <summary>Outcome of an operation that can fail without throwing; inspect <see cref="IsSuccess"/> and <see cref="Error"/>.</summary>
public class Result
{
    /// <summary>Creates a result with the given success flag and error.</summary>
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary><see langword="true"/> when the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary><see langword="true"/> when the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The failure reason; <see cref="Error.None"/> on success.</summary>
    public Error Error { get; }

    /// <summary>Creates a successful result without a value.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result carrying the given error.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a successful result wrapping <paramref name="value"/>.</summary>
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);

    /// <summary>Creates a failed result; the value defaults to <see langword="null"/>.</summary>
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

/// <summary>A <see cref="Result"/> that carries a value on success.</summary>
/// <typeparam name="T">Type of the success value.</typeparam>
public class Result<T> : Result
{
    private readonly T _value;

    /// <summary>Creates a result with a value and an outcome flag.</summary>
    internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error) => _value = value;

    /// <summary>The success value; reading it on a failed result throws.</summary>
    public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>Wraps a value as a successful result.</summary>
    public static implicit operator Result<T>(T value) => Success(value);
}
