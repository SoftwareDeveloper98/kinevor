using Kinevor.Platform.Application.Common;

namespace Kinevor.Modules.Trainers.Application;

/// <summary>Creates a trainer profile.</summary>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="Email">E-mail address.</param>
/// <param name="Phone">Phone number.</param>
/// <param name="Bio">Short biography.</param>
/// <param name="Specializations">Class types the trainer can deliver.</param>
/// <param name="Certifications">Certification labels.</param>
/// <param name="YearsExperience">Years of professional experience.</param>
/// <param name="PhotoUrl">Optional profile photo URL.</param>
public record CreateTrainerCommand(
    string FirstName, string LastName, string Email, string Phone,
    string Bio, List<string> Specializations, List<string> Certifications,
    int YearsExperience, string? PhotoUrl) : ICommand<Guid>;
