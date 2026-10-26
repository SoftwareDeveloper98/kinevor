using System.Net.Mail;
using Kinevor.Platform.Application.Exceptions;
using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.PublicSite.Domain;

/// <summary>Lifecycle shared by trial requests and website enquiries.</summary>
public enum LeadLifecycle
{
    /// <summary>Newly captured, not yet contacted.</summary>
    New,

    /// <summary>Staff have made contact.</summary>
    Contacted,

    /// <summary>Converted into a booking or membership.</summary>
    Booked,

    /// <summary>No further action planned.</summary>
    Closed,
}

/// <summary>General website enquiry captured by the contact form.</summary>
public sealed class Lead : AggregateRoot
{
    private Lead()
    {
    }

    /// <summary>Prospect name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Normalized contact e-mail.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Optional contact phone.</summary>
    public string? Phone { get; private set; }

    /// <summary>Topic the prospect selected.</summary>
    public string Interest { get; private set; } = string.Empty;

    /// <summary>Free-text enquiry.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>How the lead reached the site.</summary>
    public string Source { get; private set; } = "website";

    /// <summary>Current follow-up state.</summary>
    public LeadLifecycle Status { get; private set; } = LeadLifecycle.New;

    /// <summary>Creates a validated website enquiry.</summary>
    public static Lead Create(
        string name,
        string email,
        string? phone,
        string interest,
        string message,
        bool consentToContact)
    {
        if (!consentToContact)
            throw new DomainException("Consent to be contacted is required.");

        return new Lead
        {
            Name = LeadValidation.Name(name),
            Email = LeadValidation.Email(email),
            Phone = LeadValidation.OptionalPhone(phone),
            Interest = LeadValidation.Text(interest, nameof(interest), 120),
            Message = LeadValidation.Text(message, nameof(message), 2000, 10),
            Status = LeadLifecycle.New,
        };
    }

    /// <summary>Moves the lead to a new follow-up state.</summary>
    public void UpdateStatus(LeadLifecycle status) => Status = status;

    /// <summary>Seeds a historical enquiry for the standalone demo.</summary>
    public static Lead Seed(
        Guid id,
        string name,
        string email,
        string? phone,
        string interest,
        string message,
        string source,
        LeadLifecycle status,
        DateTime createdAt)
    {
        var lead = Create(name, email, phone, interest, message, true);
        lead.Id = id;
        lead.Source = LeadValidation.Text(source, nameof(source), 60);
        lead.Status = status;
        lead.CreatedAt = createdAt;
        return lead;
    }
}

/// <summary>Free-trial request linked to an optional class and branch.</summary>
public sealed class TrialBooking : AggregateRoot
{
    private TrialBooking()
    {
    }

    /// <summary>Prospect first name.</summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>Prospect last name.</summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>Normalized contact e-mail.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Contact phone number.</summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>Requested trial date in gym-local time.</summary>
    public DateTime? PreferredDate { get; private set; }

    /// <summary>Optional selected class identifier.</summary>
    public Guid? ClassSessionId { get; private set; }

    /// <summary>Selected class name captured at request time.</summary>
    public string? ClassName { get; private set; }

    /// <summary>Preferred branch identifier.</summary>
    public Guid? BranchId { get; private set; }

    /// <summary>Preferred branch name captured at request time.</summary>
    public string? BranchName { get; private set; }

    /// <summary>Prospect training goal.</summary>
    public string? Goal { get; private set; }

    /// <summary>Additional notes for the coaching team.</summary>
    public string? Notes { get; private set; }

    /// <summary>Current follow-up state.</summary>
    public LeadLifecycle Status { get; private set; } = LeadLifecycle.New;

    /// <summary>Full prospect name.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>Creates a validated trial request.</summary>
    public static TrialBooking Create(
        DateTime now,
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime? preferredDate,
        Guid? classSessionId,
        string? className,
        Guid? branchId,
        string? branchName,
        string? goal,
        string? notes,
        bool consentToContact)
    {
        if (!consentToContact)
            throw new DomainException("Consent to be contacted is required.");
        if (preferredDate.HasValue && preferredDate.Value.Date < now.Date)
            throw new DomainException("Preferred date cannot be in the past.");

        return new TrialBooking
        {
            FirstName = LeadValidation.Text(firstName, nameof(firstName), 80),
            LastName = LeadValidation.Text(lastName, nameof(lastName), 80),
            Email = LeadValidation.Email(email),
            Phone = LeadValidation.Phone(phone),
            PreferredDate = preferredDate,
            ClassSessionId = classSessionId,
            ClassName = LeadValidation.OptionalText(className, 160),
            BranchId = branchId,
            BranchName = LeadValidation.OptionalText(branchName, 160),
            Goal = LeadValidation.OptionalText(goal, 500),
            Notes = LeadValidation.OptionalText(notes, 1000),
            Status = LeadLifecycle.New,
        };
    }

    /// <summary>Moves the trial request to a new follow-up state.</summary>
    public void UpdateStatus(LeadLifecycle status) => Status = status;

    /// <summary>Seeds a historical trial request for the standalone demo.</summary>
    public static TrialBooking Seed(
        Guid id,
        DateTime now,
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime? preferredDate,
        Guid? classSessionId,
        string? className,
        Guid? branchId,
        string? branchName,
        string? goal,
        string? notes,
        LeadLifecycle status,
        DateTime createdAt)
    {
        var trial = Create(
            now,
            firstName,
            lastName,
            email,
            phone,
            preferredDate,
            classSessionId,
            className,
            branchId,
            branchName,
            goal,
            notes,
            true);
        trial.Id = id;
        trial.Status = status;
        trial.CreatedAt = createdAt;
        return trial;
    }
}

internal static class LeadValidation
{
    public static string Name(string value) => Text(value, "name", 160);

    public static string Email(string value)
    {
        var normalized = Text(value, "email", 200).ToLowerInvariant();
        if (!MailAddress.TryCreate(normalized, out var address) || address.Address != normalized)
            throw new DomainException("A valid e-mail address is required.");
        return normalized;
    }

    public static string Phone(string value)
    {
        var normalized = Text(value, "phone", 40, 7);
        if (normalized.Count(char.IsDigit) < 7)
            throw new DomainException("A valid phone number is required.");
        return normalized;
    }

    public static string? OptionalPhone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Phone(value);

    public static string? OptionalText(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Text(value, "value", maxLength);

    public static string Text(string value, string field, int maxLength, int minLength = 1)
    {
        var normalized = value.Trim();
        if (normalized.Length < minLength)
            throw new DomainException($"{field} is required.");
        if (normalized.Length > maxLength)
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return normalized;
    }
}
