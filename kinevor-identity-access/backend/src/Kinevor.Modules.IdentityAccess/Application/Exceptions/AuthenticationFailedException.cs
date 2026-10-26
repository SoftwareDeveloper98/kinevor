namespace Kinevor.Modules.IdentityAccess.Application.Exceptions;

/// <summary>Thrown when credentials or a refresh token are invalid; mapped to 401 by the host middleware.</summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);
