namespace Kinevor.Platform.Application.Exceptions;

/// <summary>Thrown when a requested entity does not exist; mapped to 404 by the host API middleware.</summary>
public sealed class NotFoundException(string name, object key)
    : Exception($"{name} with key '{key}' was not found.");
