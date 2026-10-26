using Kinevor.Modules.Notifications.Domain;
using MediatR;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>Known preference category with its default channel flags.</summary>
public record NotificationCategoryDto(string Key, bool Email, bool Push, bool InApp);

/// <summary>Lists the seeded message templates available to staff.</summary>
public record GetNotificationTemplatesQuery : IRequest<IReadOnlyList<NotificationTemplateDto>>;

/// <summary>Handles <see cref="GetNotificationTemplatesQuery"/>.</summary>
public class GetNotificationTemplatesHandler(INotificationTemplateRepository templates)
    : IRequestHandler<GetNotificationTemplatesQuery, IReadOnlyList<NotificationTemplateDto>>
{
    /// <summary>Returns every template ordered by key.</summary>
    public async Task<IReadOnlyList<NotificationTemplateDto>> Handle(
        GetNotificationTemplatesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var items = await templates.ListAsync(ct);
        return items.Select(NotificationMapper.ToDto).ToList();
    }
}

/// <summary>Lists the categories members can configure.</summary>
public record GetNotificationCategoriesQuery : IRequest<IReadOnlyList<NotificationCategoryDto>>;

/// <summary>Handles <see cref="GetNotificationCategoriesQuery"/>.</summary>
public class GetNotificationCategoriesHandler : IRequestHandler<GetNotificationCategoriesQuery, IReadOnlyList<NotificationCategoryDto>>
{
    /// <summary>Returns every known category with the default enabled flags.</summary>
    public Task<IReadOnlyList<NotificationCategoryDto>> Handle(
        GetNotificationCategoriesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        IReadOnlyList<NotificationCategoryDto> categories = NotificationCategory.All
            .Select(key => new NotificationCategoryDto(key, true, true, true))
            .ToList();

        return Task.FromResult(categories);
    }
}
