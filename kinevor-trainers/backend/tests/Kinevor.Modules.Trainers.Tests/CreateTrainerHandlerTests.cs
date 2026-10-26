using Kinevor.Modules.Trainers.Application;
using Kinevor.Platform.Application.Exceptions;
using Kinevor.Platform.Domain.Enums;

namespace Kinevor.Modules.Trainers.Tests;

/// <summary>Application behavior for trainer creation and reads.</summary>
public class CreateTrainerHandlerTests
{
    /// <summary>Creation adds the trainer, assigns the branch and saves once.</summary>
    [Fact]
    public async Task Create_AddsTrainerAndSaves()
    {
        var repo = new FakeTrainerRepository();
        var uow = new FakeUnitOfWork();
        var branchId = Guid.NewGuid();
        var handler = new CreateTrainerHandler(repo, uow, new FakeBranchContext(branchId));

        var id = await handler.Handle(new CreateTrainerCommand(
            "Layla", "Hassan", "layla@kinevor.demo", "+201001234567", "Yoga coach",
            ["Yoga", "Pilates"], ["RYT-500"], 8, null), CancellationToken.None);

        var trainer = Assert.Single(repo.Items);
        Assert.Equal(id, trainer.Id);
        Assert.Equal(branchId, trainer.BranchId);
        Assert.Equal(1, uow.SaveCount);
        Assert.Equal([ClassType.Yoga, ClassType.Pilates], trainer.Specializations);
    }

    /// <summary>Invalid input is translated to a domain exception.</summary>
    [Fact]
    public async Task Create_RejectsInvalidEmail()
    {
        var handler = new CreateTrainerHandler(
            new FakeTrainerRepository(), new FakeUnitOfWork(), new FakeBranchContext(null));

        var action = () => handler.Handle(new CreateTrainerCommand(
            "Layla", "Hassan", "invalid", "+201001234567", "Yoga coach",
            ["Yoga"], [], 8, null), CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(action);
    }

    /// <summary>A missing trainer produces a not-found exception.</summary>
    [Fact]
    public async Task GetById_ThrowsWhenMissing()
    {
        var handler = new GetTrainerByIdHandler(
            new FakeTrainerRepository(), new FakeCurrentUser { Roles = ["Staff"] });

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetTrainerByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    /// <summary>A trainer may only read their own profile.</summary>
    [Fact]
    public async Task GetById_ForbidsOtherTrainerProfile()
    {
        var repo = new FakeTrainerRepository();
        var create = new CreateTrainerHandler(repo, new FakeUnitOfWork(), new FakeBranchContext(null));
        var id = await create.Handle(new CreateTrainerCommand(
            "Layla", "Hassan", "layla@kinevor.demo", "+201001234567", "Yoga coach",
            ["Yoga"], [], 8, null), CancellationToken.None);

        var handler = new GetTrainerByIdHandler(
            repo, new FakeCurrentUser { Roles = ["Trainer"], TrainerId = Guid.NewGuid() });

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GetTrainerByIdQuery(id), CancellationToken.None));
    }
}
