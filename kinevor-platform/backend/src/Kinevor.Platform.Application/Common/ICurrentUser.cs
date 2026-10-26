namespace Kinevor.Platform.Application.Common;

/// <summary>Identity of the authenticated caller for authorization and object-level checks.</summary>
public interface ICurrentUser
{
    /// <summary>Identity user id.</summary>
    Guid? UserId { get; }

    /// <summary>Roles held by the user.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>E-mail address of the user.</summary>
    string? Email { get; }

    /// <summary>Home branch claim, when set.</summary>
    Guid? BranchId { get; }

    /// <summary>Linked Member aggregate id (claim <c>member_id</c>), for member object-level checks.</summary>
    Guid? MemberId { get; }

    /// <summary>Linked Trainer aggregate id (claim <c>trainer_id</c>), for trainer ownership checks.</summary>
    Guid? TrainerId { get; }
}
