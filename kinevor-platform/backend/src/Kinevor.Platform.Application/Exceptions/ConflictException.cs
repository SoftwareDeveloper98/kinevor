namespace Kinevor.Platform.Application.Exceptions;

/// <summary>Thrown when the request conflicts with current state; mapped to 409 by the host API middleware.</summary>
public sealed class ConflictException(string message) : Exception(message);
