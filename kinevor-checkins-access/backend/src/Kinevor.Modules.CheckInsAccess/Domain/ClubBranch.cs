using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.CheckInsAccess.Domain;

/// <summary>Branch reference and safe occupancy capacity for the access module.</summary>
public sealed class ClubBranch : AggregateRoot
{
    private ClubBranch()
    {
    }

    /// <summary>Branch display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Short branch code used by staff applications.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>City shown in branch selectors.</summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>Maximum safe occupancy used for utilization snapshots.</summary>
    public int Capacity { get; private set; }

    /// <summary>Creates a validated branch.</summary>
    /// <param name="name">Branch display name.</param>
    /// <param name="code">Short branch code.</param>
    /// <param name="city">City name.</param>
    /// <param name="capacity">Safe occupancy capacity.</param>
    /// <returns>The created branch, or a validation failure.</returns>
    public static Result<ClubBranch> Create(string name, string code, string city, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ClubBranch>(Error.Validation("Name", "Branch name is required."));
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<ClubBranch>(Error.Validation("Code", "Branch code is required."));
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<ClubBranch>(Error.Validation("City", "City is required."));
        if (capacity is < 1 or > 5000)
            return Result.Failure<ClubBranch>(Error.Validation("Capacity", "Capacity must be between 1 and 5000."));

        return Result.Success(new ClubBranch
        {
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            City = city.Trim(),
            Capacity = capacity,
        });
    }
}
