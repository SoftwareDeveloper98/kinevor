using Kinevor.Modules.Notifications.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>Lists the authenticated member's registered push devices.</summary>
public record GetMyDeviceTokensQuery(Guid MemberId) : IRequest<IReadOnlyList<DeviceTokenDto>>;

/// <summary>Handles <see cref="GetMyDeviceTokensQuery"/>.</summary>
public class GetMyDeviceTokensHandler(IDeviceTokenRepository devices)
    : IRequestHandler<GetMyDeviceTokensQuery, IReadOnlyList<DeviceTokenDto>>
{
    /// <summary>Returns masked device projections newest first.</summary>
    public async Task<IReadOnlyList<DeviceTokenDto>> Handle(GetMyDeviceTokensQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var items = await devices.ListAsync(query.MemberId, ct);
        return items.Select(NotificationMapper.ToDto).ToList();
    }
}

/// <summary>Registers or re-assigns a push device token for the authenticated member.</summary>
public record RegisterDeviceTokenCommand(Guid MemberId, string Token, string Platform)
    : ICommand<DeviceTokenDto>;

/// <summary>Handles <see cref="RegisterDeviceTokenCommand"/>.</summary>
public class RegisterDeviceTokenHandler(
    IDeviceTokenRepository devices,
    INotificationsUnitOfWork unitOfWork) : IRequestHandler<RegisterDeviceTokenCommand, DeviceTokenDto>
{
    /// <summary>Creates the token or moves an existing token to the current member.</summary>
    public async Task<DeviceTokenDto> Handle(RegisterDeviceTokenCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = await devices.FindByTokenAsync(command.Token, ct);
        if (existing is not null)
        {
            existing.Assign(command.MemberId, command.Platform);
            await unitOfWork.SaveChangesAsync(ct);
            return NotificationMapper.ToDto(existing);
        }

        var created = DeviceToken.Register(command.MemberId, command.Token, command.Platform);
        if (created.IsFailure) throw new DomainException(created.Error.Message);

        devices.Add(created.Value);
        await unitOfWork.SaveChangesAsync(ct);
        return NotificationMapper.ToDto(created.Value);
    }
}

/// <summary>Unregisters one of the authenticated member's devices.</summary>
public record UnregisterDeviceTokenCommand(Guid MemberId, Guid DeviceId) : ICommand;

/// <summary>Handles <see cref="UnregisterDeviceTokenCommand"/>.</summary>
public class UnregisterDeviceTokenHandler(
    IDeviceTokenRepository devices,
    INotificationsUnitOfWork unitOfWork) : IRequestHandler<UnregisterDeviceTokenCommand>
{
    /// <summary>Deletes the device when it belongs to the member.</summary>
    public async Task Handle(UnregisterDeviceTokenCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var owned = await devices.ListAsync(command.MemberId, ct);
        var device = owned.FirstOrDefault(item => item.Id == command.DeviceId)
            ?? throw new NotFoundException("DeviceToken", command.DeviceId);

        devices.Remove(device);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
