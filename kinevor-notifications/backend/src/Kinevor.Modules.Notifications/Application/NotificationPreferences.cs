using Kinevor.Modules.Notifications.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>One category's requested channel flags.</summary>
public record NotificationPreferenceInput(string Category, bool Email, bool Push, bool InApp);

/// <summary>Lists the authenticated member's preferences for every known category.</summary>
public record GetNotificationPreferencesQuery(Guid MemberId)
    : IRequest<IReadOnlyList<NotificationPreferenceDto>>;

/// <summary>Handles <see cref="GetNotificationPreferencesQuery"/>.</summary>
public class GetNotificationPreferencesHandler(INotificationPreferenceRepository preferences)
    : IRequestHandler<GetNotificationPreferencesQuery, IReadOnlyList<NotificationPreferenceDto>>
{
    /// <summary>Returns stored rows and defaults missing categories to enabled.</summary>
    public async Task<IReadOnlyList<NotificationPreferenceDto>> Handle(
        GetNotificationPreferencesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var stored = (await preferences.ListAsync(query.MemberId, ct))
            .ToDictionary(item => item.Category, StringComparer.OrdinalIgnoreCase);

        return NotificationCategory.All
            .Select(category => stored.TryGetValue(category, out var preference)
                ? NotificationMapper.ToDto(preference)
                : new NotificationPreferenceDto(category, true, true, true))
            .ToList();
    }
}

/// <summary>Upserts the authenticated member's preferences by category.</summary>
public record UpdateNotificationPreferencesCommand(
    Guid MemberId,
    IReadOnlyList<NotificationPreferenceInput> Preferences) : ICommand;

/// <summary>Handles <see cref="UpdateNotificationPreferencesCommand"/>.</summary>
public class UpdateNotificationPreferencesHandler(
    INotificationPreferenceRepository preferences,
    INotificationsUnitOfWork unitOfWork) : IRequestHandler<UpdateNotificationPreferencesCommand>
{
    /// <summary>Validates every category and applies the channel flags.</summary>
    public async Task Handle(UpdateNotificationPreferencesCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Preferences is null || command.Preferences.Count == 0)
            throw new DomainException("At least one notification preference is required.");

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var input in command.Preferences)
        {
            if (string.IsNullOrWhiteSpace(input.Category))
                throw new DomainException("A preference category is required.");

            var category = NotificationCategory.Normalize(input.Category);
            if (!NotificationCategory.IsKnown(category))
                throw new DomainException($"Unknown notification category '{input.Category}'.");

            if (!seen.Add(category))
                throw new DomainException($"Duplicate notification preference for category '{category}'.");

            var preference = await preferences.FindAsync(command.MemberId, category, ct);
            if (preference is null)
            {
                preference = NotificationPreference.For(command.MemberId, category);
                preferences.Add(preference);
            }

            preference.Update(input.Email, input.Push, input.InApp);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
