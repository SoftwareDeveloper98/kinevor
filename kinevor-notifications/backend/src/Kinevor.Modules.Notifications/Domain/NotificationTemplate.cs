using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.Notifications.Domain;

/// <summary>Reusable message template used by staff to compose notifications.</summary>
public sealed class NotificationTemplate : Entity
{
    private NotificationTemplate()
    {
    }

    /// <summary>Stable machine key such as <c>booking_confirmed</c>.</summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>Human-readable template name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Preference category applied to notifications rendered from this template.</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Channel used when the staff request does not choose one.</summary>
    public NotificationChannel DefaultChannel { get; private set; }

    /// <summary>Title template with <c>{Placeholder}</c> tokens.</summary>
    public string TitleTemplate { get; private set; } = string.Empty;

    /// <summary>Body template with <c>{Placeholder}</c> tokens.</summary>
    public string BodyTemplate { get; private set; } = string.Empty;

    /// <summary>Whether the template may be used for new messages.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Validates and creates a template.</summary>
    public static Result<NotificationTemplate> Create(
        string key,
        string name,
        string category,
        NotificationChannel defaultChannel,
        string titleTemplate,
        string bodyTemplate)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure<NotificationTemplate>(Error.Validation("Key", "Template key is required."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<NotificationTemplate>(Error.Validation("Name", "Template name is required."));

        if (!NotificationCategory.IsKnown(category))
            return Result.Failure<NotificationTemplate>(Error.Validation("Category",
                $"Unknown notification category '{category}'."));

        if (string.IsNullOrWhiteSpace(titleTemplate))
            return Result.Failure<NotificationTemplate>(Error.Validation("TitleTemplate", "Title template is required."));

        if (string.IsNullOrWhiteSpace(bodyTemplate))
            return Result.Failure<NotificationTemplate>(Error.Validation("BodyTemplate", "Body template is required."));

        return Result.Success(new NotificationTemplate
        {
            Key = key.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Category = NotificationCategory.Normalize(category),
            DefaultChannel = defaultChannel,
            TitleTemplate = titleTemplate.Trim(),
            BodyTemplate = bodyTemplate.Trim(),
        });
    }

    /// <summary>Renders the template, replacing <c>{Placeholder}</c> tokens from <paramref name="values"/>.</summary>
    public (string Title, string Body) Render(IReadOnlyDictionary<string, string>? values)
    {
        var title = TitleTemplate;
        var body = BodyTemplate;

        if (values is not null)
        {
            foreach (var (name, value) in values)
            {
                title = title.Replace("{" + name + "}", value, StringComparison.OrdinalIgnoreCase);
                body = body.Replace("{" + name + "}", value, StringComparison.OrdinalIgnoreCase);
            }
        }

        return (title, body);
    }
}
