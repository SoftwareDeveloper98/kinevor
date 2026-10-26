namespace Kinevor.Platform.Application.Exceptions;

/// <summary>Thrown when a domain rule is violated; mapped to 400 by the host API middleware.</summary>
public sealed class DomainException(string message) : Exception(message);
