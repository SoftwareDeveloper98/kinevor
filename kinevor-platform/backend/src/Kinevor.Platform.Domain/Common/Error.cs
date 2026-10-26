namespace Kinevor.Platform.Domain.Common;

/// <summary>Typed failure with a stable machine-readable code and a human-readable message.</summary>
public sealed record Error(string Code, string Message)
{
    /// <summary>Sentinel carried by successful results.</summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>Creates a not-found error for an entity identified by id.</summary>
    public static Error NotFound(string entity, Guid id) =>
        new($"{entity}.NotFound", $"{entity} with id '{id}' was not found.");

    /// <summary>Creates an error for a request that conflicts with current state.</summary>
    public static Error Conflict(string message) => new("Conflict", message);

    /// <summary>Creates a field-level validation error.</summary>
    public static Error Validation(string field, string message) => new($"Validation.{field}", message);

    /// <summary>Creates a business-rule violation with a caller-supplied code.</summary>
    public static Error Business(string code, string message) => new(code, message);
}
