namespace Kinevor.Platform.Api;

/// <summary>Standard API envelope carrying a data payload.</summary>
/// <typeparam name="T">Payload type.</typeparam>
public record ApiResponse<T>(bool Success, T? Data, string? Message = null, IEnumerable<string>? Errors = null)
{
    /// <summary>Builds a successful envelope.</summary>
    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, data, message);

    /// <summary>Builds a failed envelope.</summary>
    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) => new(false, default, message, errors);
}

/// <summary>Standard API envelope without a data payload.</summary>
public record ApiResponse(bool Success, string? Message = null, IEnumerable<string>? Errors = null)
{
    /// <summary>Builds a successful envelope.</summary>
    public static ApiResponse Ok(string? message = null) => new(true, message);

    /// <summary>Builds a failed envelope.</summary>
    public static ApiResponse Fail(string message, IEnumerable<string>? errors = null) => new(false, message, errors);
}
