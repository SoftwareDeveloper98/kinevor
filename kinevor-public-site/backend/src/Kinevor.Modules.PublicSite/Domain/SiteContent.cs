using Kinevor.Platform.Application.Exceptions;
using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.PublicSite.Domain;

/// <summary>Singleton editorial profile that powers the public website's brand and contact surfaces.</summary>
public sealed class SiteProfile : AggregateRoot
{
    private SiteProfile()
    {
    }

    /// <summary>Public gym name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Short brand promise used in the hero.</summary>
    public string Tagline { get; private set; } = string.Empty;

    /// <summary>One-sentence positioning statement.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Longer story shown in the about section.</summary>
    public string Story { get; private set; } = string.Empty;

    /// <summary>Small label above the hero headline.</summary>
    public string Eyebrow { get; private set; } = string.Empty;

    /// <summary>Front-desk phone number.</summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>Public contact e-mail address.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Head-office street address.</summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>Head-office city and region.</summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>Instagram handle, including the leading at sign.</summary>
    public string InstagramHandle { get; private set; } = string.Empty;

    /// <summary>Current member count shown in social proof.</summary>
    public int MemberCount { get; private set; }

    /// <summary>Current coach count shown in social proof.</summary>
    public int TrainerCount { get; private set; }

    /// <summary>Classes run across all branches each week.</summary>
    public int WeeklyClasses { get; private set; }

    /// <summary>Average member rating out of five.</summary>
    public decimal Rating { get; private set; }

    /// <summary>Year the first Kinevor gym opened.</summary>
    public int FoundedYear { get; private set; }

    /// <summary>Short proof points displayed beneath the hero.</summary>
    public IReadOnlyList<string> Highlights { get; private set; } = [];

    /// <summary>Creates the seeded site profile.</summary>
    public static SiteProfile Seed(
        Guid id,
        string name,
        string tagline,
        string description,
        string story,
        string eyebrow,
        string phone,
        string email,
        string address,
        string city,
        string instagramHandle,
        int memberCount,
        int trainerCount,
        int weeklyClasses,
        decimal rating,
        int foundedYear,
        IReadOnlyList<string> highlights)
    {
        return new SiteProfile
        {
            Id = id,
            Name = Required(name, nameof(name), 120),
            Tagline = Required(tagline, nameof(tagline), 160),
            Description = Required(description, nameof(description), 600),
            Story = Required(story, nameof(story), 2000),
            Eyebrow = Required(eyebrow, nameof(eyebrow), 120),
            Phone = Required(phone, nameof(phone), 40),
            Email = Required(email, nameof(email), 200),
            Address = Required(address, nameof(address), 240),
            City = Required(city, nameof(city), 120),
            InstagramHandle = Required(instagramHandle, nameof(instagramHandle), 80),
            MemberCount = Math.Max(0, memberCount),
            TrainerCount = Math.Max(0, trainerCount),
            WeeklyClasses = Math.Max(0, weeklyClasses),
            Rating = Math.Clamp(rating, 0m, 5m),
            FoundedYear = foundedYear,
            Highlights = highlights.ToArray(),
        };
    }

    private static string Required(string value, string field, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            throw new DomainException($"{field} is required.");
        if (normalized.Length > maxLength)
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return normalized;
    }
}

/// <summary>Public gym location with contact, opening and amenity information.</summary>
public sealed class Branch : AggregateRoot
{
    private Branch()
    {
    }

    /// <summary>Display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>URL-safe location slug.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Neighbourhood or short location label.</summary>
    public string Neighbourhood { get; private set; } = string.Empty;

    /// <summary>Street address.</summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>City and region.</summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>Front-desk phone number.</summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>Human-readable opening hours.</summary>
    public string OpeningHours { get; private set; } = string.Empty;

    /// <summary>Parking guidance.</summary>
    public string Parking { get; private set; } = string.Empty;

    /// <summary>Facilities available at this location.</summary>
    public IReadOnlyList<string> Amenities { get; private set; } = [];

    /// <summary>Visitor capacity.</summary>
    public int Capacity { get; private set; }

    /// <summary>Latitude for map integrations.</summary>
    public double Latitude { get; private set; }

    /// <summary>Longitude for map integrations.</summary>
    public double Longitude { get; private set; }

    /// <summary>Presentation tone consumed by the web client.</summary>
    public string Theme { get; private set; } = "ember";

    /// <summary>Whether the branch appears on the public site.</summary>
    public bool IsPublished { get; private set; } = true;

    /// <summary>Display order across location surfaces.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Creates a validated location.</summary>
    public static Branch Seed(
        Guid id,
        string name,
        string slug,
        string neighbourhood,
        string address,
        string city,
        string phone,
        string openingHours,
        string parking,
        IReadOnlyList<string> amenities,
        int capacity,
        double latitude,
        double longitude,
        string theme,
        int sortOrder,
        bool isPublished = true)
    {
        return new Branch
        {
            Id = id,
            Name = Text(name, nameof(name), 160),
            Slug = Slugify(slug),
            Neighbourhood = Text(neighbourhood, nameof(neighbourhood), 120),
            Address = Text(address, nameof(address), 240),
            City = Text(city, nameof(city), 120),
            Phone = Text(phone, nameof(phone), 40),
            OpeningHours = Text(openingHours, nameof(openingHours), 240),
            Parking = Text(parking, nameof(parking), 500),
            Amenities = amenities.Select(item => Text(item, nameof(amenities), 80)).ToArray(),
            Capacity = Math.Max(1, capacity),
            Latitude = latitude,
            Longitude = longitude,
            Theme = Text(theme, nameof(theme), 30),
            SortOrder = sortOrder,
            IsPublished = isPublished,
        };
    }

    private static string Text(string value, string field, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            throw new DomainException($"{field} is required.");
        if (normalized.Length > maxLength)
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return normalized;
    }

    private static string Slugify(string value)
    {
        var normalized = Text(value, nameof(value), 120).ToLowerInvariant();
        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
            throw new DomainException("slug may only contain letters, digits, hyphens and underscores.");
        return normalized;
    }
}

/// <summary>Grouped answer to a common pre-sales or membership question.</summary>
public sealed class FaqEntry : AggregateRoot
{
    private FaqEntry()
    {
    }

    /// <summary>Question group shown as a filter.</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Question text.</summary>
    public string Question { get; private set; } = string.Empty;

    /// <summary>Plain-text answer.</summary>
    public string Answer { get; private set; } = string.Empty;

    /// <summary>Display order within the FAQ list.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Whether the answer is visible publicly.</summary>
    public bool IsPublished { get; private set; } = true;

    /// <summary>Creates a validated FAQ entry.</summary>
    public static FaqEntry Seed(
        Guid id,
        string category,
        string question,
        string answer,
        int sortOrder,
        bool isPublished = true)
    {
        return new FaqEntry
        {
            Id = id,
            Category = Text(category, nameof(category), 80),
            Question = Text(question, nameof(question), 300),
            Answer = Text(answer, nameof(answer), 2000),
            SortOrder = sortOrder,
            IsPublished = isPublished,
        };
    }

    private static string Text(string value, string field, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            throw new DomainException($"{field} is required.");
        if (normalized.Length > maxLength)
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return normalized;
    }
}

/// <summary>Editorial article published on the public website.</summary>
public sealed class BlogPost : AggregateRoot
{
    private BlogPost()
    {
    }

    /// <summary>URL-safe article slug.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Article title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Lead paragraph used on index cards.</summary>
    public string Excerpt { get; private set; } = string.Empty;

    /// <summary>HTML article body.</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>Optional cover image URL.</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Author display name.</summary>
    public string Author { get; private set; } = string.Empty;

    /// <summary>Publication timestamp.</summary>
    public DateTime PublishedAt { get; private set; }

    /// <summary>Whether the article is visible publicly.</summary>
    public bool IsPublished { get; private set; } = true;

    /// <summary>Creates a validated article.</summary>
    public static BlogPost Seed(
        Guid id,
        string slug,
        string title,
        string excerpt,
        string content,
        string author,
        DateTime publishedAt,
        bool isPublished = true,
        string? imageUrl = null)
    {
        return new BlogPost
        {
            Id = id,
            Slug = Text(slug, nameof(slug), 160).ToLowerInvariant(),
            Title = Text(title, nameof(title), 240),
            Excerpt = Text(excerpt, nameof(excerpt), 600),
            Content = Text(content, nameof(content), 20000),
            Author = Text(author, nameof(author), 120),
            PublishedAt = publishedAt,
            IsPublished = isPublished,
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim(),
        };
    }

    private static string Text(string value, string field, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            throw new DomainException($"{field} is required.");
        if (normalized.Length > maxLength)
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return normalized;
    }
}
