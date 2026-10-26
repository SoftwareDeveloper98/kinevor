using Kinevor.Modules.Notifications.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>Lists suppressed e-mail addresses.</summary>
public record GetEmailSuppressionsQuery(string? Search = null)
    : IRequest<IReadOnlyList<EmailSuppressionDto>>;

/// <summary>Handles <see cref="GetEmailSuppressionsQuery"/>.</summary>
public class GetEmailSuppressionsHandler(IEmailSuppressionRepository suppressions)
    : IRequestHandler<GetEmailSuppressionsQuery, IReadOnlyList<EmailSuppressionDto>>
{
    /// <summary>Returns suppressions ordered by address.</summary>
    public async Task<IReadOnlyList<EmailSuppressionDto>> Handle(
        GetEmailSuppressionsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var items = await suppressions.ListAsync(query.Search, ct);
        return items.Select(NotificationMapper.ToDto).ToList();
    }
}

/// <summary>Records or updates an e-mail suppression.</summary>
public record AddEmailSuppressionCommand(string Email, string Reason, string? Detail = null)
    : ICommand<EmailSuppressionDto>;

/// <summary>Handles <see cref="AddEmailSuppressionCommand"/>.</summary>
public class AddEmailSuppressionHandler(
    IEmailSuppressionRepository suppressions,
    INotificationsUnitOfWork unitOfWork) : IRequestHandler<AddEmailSuppressionCommand, EmailSuppressionDto>
{
    /// <summary>Creates the row or updates the reason of an existing address.</summary>
    public async Task<EmailSuppressionDto> Handle(AddEmailSuppressionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var created = EmailSuppression.Create(command.Email, command.Reason, command.Detail);
        if (created.IsFailure) throw new DomainException(created.Error.Message);

        var existing = await suppressions.FindAsync(created.Value.Email, ct);
        if (existing is not null)
        {
            existing.Update(command.Reason, command.Detail);
            await unitOfWork.SaveChangesAsync(ct);
            return NotificationMapper.ToDto(existing);
        }

        suppressions.Add(created.Value);
        await unitOfWork.SaveChangesAsync(ct);
        return NotificationMapper.ToDto(created.Value);
    }
}

/// <summary>Removes an e-mail suppression by address.</summary>
public record RemoveEmailSuppressionCommand(string Email) : ICommand;

/// <summary>Handles <see cref="RemoveEmailSuppressionCommand"/>.</summary>
public class RemoveEmailSuppressionHandler(
    IEmailSuppressionRepository suppressions,
    INotificationsUnitOfWork unitOfWork) : IRequestHandler<RemoveEmailSuppressionCommand>
{
    /// <summary>Deletes the suppression or reports that it does not exist.</summary>
    public async Task Handle(RemoveEmailSuppressionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new DomainException("E-mail address is required.");

        var suppression = await suppressions.FindAsync(command.Email, ct)
            ?? throw new NotFoundException("EmailSuppression", command.Email);

        suppressions.Remove(suppression);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
