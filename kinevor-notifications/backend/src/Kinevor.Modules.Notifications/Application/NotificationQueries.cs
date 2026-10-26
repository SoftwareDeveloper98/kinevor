using Kinevor.Modules.Notifications.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>Lists notifications for the authenticated member's inbox.</summary>
public record GetMyNotificationsQuery(
    Guid MemberId,
    string? Category,
    string? Channel,
    bool UnreadOnly,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationDto>>;

/// <summary>Handles <see cref="GetMyNotificationsQuery"/>.</summary>
public class GetMyNotificationsHandler(INotificationRepository notifications)
    : IRequestHandler<GetMyNotificationsQuery, PagedResult<NotificationDto>>
{
    /// <summary>Returns the matching page of the member's notification records.</summary>
    public async Task<PagedResult<NotificationDto>> Handle(GetMyNotificationsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var (items, total) = await notifications.SearchAsync(
            query.MemberId,
            query.Category,
            query.Channel,
            null,
            query.UnreadOnly,
            page,
            pageSize,
            ct);

        return new PagedResult<NotificationDto>(
            items.Select(NotificationMapper.ToDto).ToList(),
            total,
            page,
            pageSize);
    }
}

/// <summary>Lists the authenticated member's unread notifications.</summary>
public record GetMyUnreadNotificationsQuery(Guid MemberId) : IRequest<IReadOnlyList<NotificationDto>>;

/// <summary>Handles <see cref="GetMyUnreadNotificationsQuery"/>.</summary>
public class GetMyUnreadNotificationsHandler(INotificationRepository notifications)
    : IRequestHandler<GetMyUnreadNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    /// <summary>Returns unread notifications newest first.</summary>
    public async Task<IReadOnlyList<NotificationDto>> Handle(GetMyUnreadNotificationsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var items = await notifications.ListUnreadAsync(query.MemberId, ct);
        return items.Select(NotificationMapper.ToDto).ToList();
    }
}

/// <summary>Counts the authenticated member's unread notifications.</summary>
public record GetMyUnreadCountQuery(Guid MemberId) : IRequest<UnreadCountDto>;

/// <summary>Handles <see cref="GetMyUnreadCountQuery"/>.</summary>
public class GetMyUnreadCountHandler(INotificationRepository notifications)
    : IRequestHandler<GetMyUnreadCountQuery, UnreadCountDto>
{
    /// <summary>Returns the unread count.</summary>
    public async Task<UnreadCountDto> Handle(GetMyUnreadCountQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var count = await notifications.CountUnreadAsync(query.MemberId, ct);
        return new UnreadCountDto(count);
    }
}

/// <summary>Returns one notification belonging to the authenticated member.</summary>
public record GetMyNotificationQuery(Guid MemberId, Guid NotificationId) : IRequest<NotificationDto>;

/// <summary>Handles <see cref="GetMyNotificationQuery"/>.</summary>
public class GetMyNotificationHandler(INotificationRepository notifications)
    : IRequestHandler<GetMyNotificationQuery, NotificationDto>
{
    /// <summary>Loads the record and rejects notifications owned by other members.</summary>
    public async Task<NotificationDto> Handle(GetMyNotificationQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var notification = await notifications.GetByIdAsync(query.NotificationId, ct);
        if (notification is null || notification.MemberId != query.MemberId)
            throw new NotFoundException("Notification", query.NotificationId);

        return NotificationMapper.ToDto(notification);
    }
}

/// <summary>Marks one of the authenticated member's notifications as read.</summary>
public record MarkNotificationReadCommand(Guid MemberId, Guid NotificationId) : ICommand;

/// <summary>Handles <see cref="MarkNotificationReadCommand"/>.</summary>
public class MarkNotificationReadHandler(
    INotificationRepository notifications,
    INotificationsUnitOfWork unitOfWork,
    TimeProvider clock) : IRequestHandler<MarkNotificationReadCommand>
{
    /// <summary>Transitions a delivered record to read.</summary>
    public async Task Handle(MarkNotificationReadCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var notification = await notifications.GetByIdAsync(command.NotificationId, ct);
        if (notification is null || notification.MemberId != command.MemberId)
            throw new NotFoundException("Notification", command.NotificationId);

        var read = notification.MarkRead(clock.GetUtcNow().UtcDateTime);
        if (read.IsFailure) throw new DomainException(read.Error.Message);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

/// <summary>Marks every delivered notification of the authenticated member as read.</summary>
public record MarkAllNotificationsReadCommand(Guid MemberId) : ICommand<ReadAllResultDto>;

/// <summary>Handles <see cref="MarkAllNotificationsReadCommand"/>.</summary>
public class MarkAllNotificationsReadHandler(
    INotificationRepository notifications,
    INotificationsUnitOfWork unitOfWork,
    TimeProvider clock) : IRequestHandler<MarkAllNotificationsReadCommand, ReadAllResultDto>
{
    /// <summary>Marks each unread delivered record and returns how many changed.</summary>
    public async Task<ReadAllResultDto> Handle(MarkAllNotificationsReadCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = clock.GetUtcNow().UtcDateTime;
        var unread = await notifications.ListUnreadAsync(command.MemberId, ct);
        var marked = 0;

        foreach (var notification in unread)
        {
            if (notification.Status != NotificationStatus.Sent) continue;

            var read = notification.MarkRead(now);
            if (read.IsSuccess) marked++;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new ReadAllResultDto(marked);
    }
}

/// <summary>Filters and pages the staff notification ledger.</summary>
public record SearchNotificationsQuery(
    Guid? MemberId,
    string? Category,
    string? Channel,
    string? Status,
    bool UnreadOnly,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<NotificationDto>>;

/// <summary>Handles <see cref="SearchNotificationsQuery"/>.</summary>
public class SearchNotificationsHandler(INotificationRepository notifications)
    : IRequestHandler<SearchNotificationsQuery, PagedResult<NotificationDto>>
{
    /// <summary>Returns the matching page across every member.</summary>
    public async Task<PagedResult<NotificationDto>> Handle(SearchNotificationsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var (items, total) = await notifications.SearchAsync(
            query.MemberId,
            query.Category,
            query.Channel,
            query.Status,
            query.UnreadOnly,
            page,
            pageSize,
            ct);

        return new PagedResult<NotificationDto>(
            items.Select(NotificationMapper.ToDto).ToList(),
            total,
            page,
            pageSize);
    }
}

/// <summary>Returns any notification for the staff ledger.</summary>
public record GetNotificationForStaffQuery(Guid NotificationId) : IRequest<NotificationDto>;

/// <summary>Handles <see cref="GetNotificationForStaffQuery"/>.</summary>
public class GetNotificationForStaffHandler(INotificationRepository notifications)
    : IRequestHandler<GetNotificationForStaffQuery, NotificationDto>
{
    /// <summary>Loads the record or throws when it does not exist.</summary>
    public async Task<NotificationDto> Handle(GetNotificationForStaffQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var notification = await notifications.GetByIdAsync(query.NotificationId, ct)
            ?? throw new NotFoundException("Notification", query.NotificationId);
        return NotificationMapper.ToDto(notification);
    }
}
