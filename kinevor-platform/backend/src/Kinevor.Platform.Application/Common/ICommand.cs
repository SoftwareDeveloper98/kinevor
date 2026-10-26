using MediatR;

namespace Kinevor.Platform.Application.Common;

/// <summary>Marker for commands that return a response through the mediator pipeline.</summary>
/// <typeparam name="TResponse">Response type.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>;

/// <summary>Marker for commands that return no response body.</summary>
public interface ICommand : IRequest;
