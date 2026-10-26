namespace Kinevor.Platform.Application.Exceptions;

/// <summary>Thrown when the caller may not perform the operation; mapped to 403 by the host API middleware.</summary>
public sealed class ForbiddenException(string message) : Exception(message);
