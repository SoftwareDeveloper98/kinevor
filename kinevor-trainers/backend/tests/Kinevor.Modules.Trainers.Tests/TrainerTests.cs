using Kinevor.Modules.Trainers.Domain;
using Kinevor.Platform.Domain.Enums;

namespace Kinevor.Modules.Trainers.Tests;

/// <summary>Domain rules for the trainer aggregate.</summary>
public class TrainerTests
{
    /// <summary>Valid input is trimmed and value objects normalise their values.</summary>
    [Fact]
    public void Create_NormalisesInput()
    {
        var trainer = CreateTrainer(firstName: "  Layla ", email: "LAYLA@KINEVOR.DEMO");

        Assert.Equal("Layla", trainer.FirstName);
        Assert.Equal("Hassan", trainer.LastName);
        Assert.Equal("layla@kinevor.demo", trainer.Email.Value);
        Assert.Equal("+201001234567", trainer.Phone.Value);
        Assert.Equal(TrainerStatus.Active, trainer.Status);
    }

    /// <summary>A missing first name fails validation.</summary>
    [Fact]
    public void Create_RequiresFirstName()
    {
        var result = Trainer.Create(" ", "Hassan", "layla@kinevor.demo", "+201001234567",
            "Bio", [ClassType.Yoga], ["RYT-500"], 8);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation.FirstName", result.Error.Code);
    }

    /// <summary>Years of experience must stay in a realistic range.</summary>
    [Fact]
    public void Create_RejectsImpossibleExperience()
    {
        var result = Trainer.Create("Layla", "Hassan", "layla@kinevor.demo", "+201001234567",
            "Bio", [ClassType.Yoga], ["RYT-500"], 200);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation.YearsExperience", result.Error.Code);
    }

    /// <summary>Ratings fold into a running average and count.</summary>
    [Fact]
    public void AddRating_UpdatesAverage()
    {
        var trainer = CreateTrainer();

        trainer.AddRating(4);
        Assert.Equal(4, trainer.Rating, 3);
        Assert.Equal(1, trainer.TotalRatings);

        trainer.AddRating(5);
        Assert.Equal(4.5, trainer.Rating, 3);
        Assert.Equal(2, trainer.TotalRatings);
    }

    /// <summary>Ratings outside zero to five are rejected.</summary>
    [Fact]
    public void AddRating_RejectsOutOfRange()
    {
        var trainer = CreateTrainer();

        Assert.Throws<ArgumentOutOfRangeException>(() => trainer.AddRating(6));
    }

    /// <summary>Branch assignment is recorded.</summary>
    [Fact]
    public void AssignBranch_StoresIdentifier()
    {
        var trainer = CreateTrainer();
        var branchId = Guid.NewGuid();

        trainer.AssignBranch(branchId);

        Assert.Equal(branchId, trainer.BranchId);
    }

    private static Trainer CreateTrainer(
        string firstName = "Layla", string email = "layla.hassan@kinevor.demo")
    {
        var result = Trainer.Create(firstName, "Hassan", email, "+201001234567",
            "Yoga coach", [ClassType.Yoga, ClassType.Pilates], ["RYT-500"], 8);
        return Assert.IsType<Trainer>(result.Value);
    }
}
