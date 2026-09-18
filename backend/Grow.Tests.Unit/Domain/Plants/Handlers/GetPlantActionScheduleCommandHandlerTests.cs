using Moq;
using MockQueryable.Moq;
using Grow.Domain.Plants;
using Grow.Domain.Species;
using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Domain.Plants.Handlers;

namespace Grow.Tests.Unit.Domain.Plants.Handlers;

[TestFixture]
public class GetPlantActionScheduleCommandHandlerTests
{
    public static Mock<IDatabaseContext> CreateContextMock(Plant[] plants, Specie[] species, PlantEvent[] events)
    {
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Plants).Returns(plants.ToList().BuildMockDbSet().Object);
        _ = ctxMock.Setup(c => c.Species).Returns(species.ToList().BuildMockDbSet().Object);
        _ = ctxMock.Setup(c => c.PlantEvents).Returns(events.ToList().BuildMockDbSet().Object);
        return ctxMock;
    }

    private static Mock<IAuthUserSessionProvider> CreateUserSessionProviderMock(Guid userId)
    {
        var userSessionProviderMock = new Mock<IAuthUserSessionProvider>();
        _ = userSessionProviderMock.Setup(x => x.Get()).Returns(new AuthUser(userId, true));
        return userSessionProviderMock;
    }

    [Test]
    public async Task HandleAsync_WhenLastEventAndIntervalExists_ReturnsNextDate()
    {
        var ownerId = Guid.NewGuid();
        var specieId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-001", specieId, ownerId);

        var specie = Specie.Create(specieId, "Monstera", ownerId);
        specie.SetWateringInterval(TimeSpan.FromDays(7));

        var executedAt = new DateTime(2026, 9, 10, 18, 25, 0);
        var plantEvent = new PlantEvent(plant.Id, Guid.NewGuid(), PlantActionType.Watering, executedAt);

        var ctxMock = CreateContextMock([plant], [specie], [plantEvent]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantActionScheduleCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new GetPlantActionScheduleCommand(plant.Id, PlantActionType.Watering);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.NextDate, Is.EqualTo(new DateOnly(2026, 9, 17)));
    }

    [Test]
    public async Task HandleAsync_WhenLastEventDoesNotExist_ReturnsNull()
    {
        var ownerId = Guid.NewGuid();
        var specieId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-001", specieId, ownerId);

        var specie = Specie.Create(specieId, "Monstera", ownerId);
        specie.SetWateringInterval(TimeSpan.FromDays(7));

        var ctxMock = CreateContextMock([plant], [specie], []);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantActionScheduleCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new GetPlantActionScheduleCommand(plant.Id, PlantActionType.Watering);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.NextDate, Is.Null);
    }

    [Test]
    public async Task HandleAsync_WhenIntervalDoesNotExist_ReturnsNull()
    {
        var ownerId = Guid.NewGuid();
        var specieId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-001", specieId, ownerId);

        var specie = Specie.Create(specieId, "Monstera", ownerId);

        var plantEvent = new PlantEvent(plant.Id, Guid.NewGuid(), PlantActionType.Watering, new DateTime(2026, 9, 10, 18, 37, 0));

        var ctxMock = CreateContextMock([plant], [specie], [plantEvent]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantActionScheduleCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new GetPlantActionScheduleCommand(plant.Id, PlantActionType.Watering);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.NextDate, Is.Null);
    }

    [Test]
    public async Task HandleAsync_WhenMultipleEventsExist_UsesLatestEvent()
    {
        var ownerId = Guid.NewGuid();
        var specieId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-001", specieId, ownerId);

        var specie = Specie.Create(specieId, "Monstera", ownerId);
        specie.SetWateringInterval(TimeSpan.FromDays(7));

        var plantEventOne = new PlantEvent(plant.Id, Guid.NewGuid(), PlantActionType.Watering, new DateTime(2026, 9, 1, 18, 30, 0));
        var plantEventSecond = new PlantEvent(plant.Id, Guid.NewGuid(), PlantActionType.Watering, new DateTime(2026, 9, 10, 18, 30, 0));

        var ctxMock = CreateContextMock([plant], [specie], [plantEventOne, plantEventSecond]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantActionScheduleCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new GetPlantActionScheduleCommand(plant.Id, PlantActionType.Watering);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.NextDate, Is.EqualTo(new DateOnly(2026, 9, 17)));
    }

    [Test]
    public void HandleAsync_WhenUserIsNotPlantOwner_ThrowsOwnershipException()
    {
        var specieId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-001", specieId, Guid.NewGuid());
        var specie = Specie.Create(specieId, "Monstera", Guid.NewGuid());

        var ctxMock = CreateContextMock([plant], [specie], []);
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());

        var handler = new GetPlantActionScheduleCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new GetPlantActionScheduleCommand(plant.Id, PlantActionType.Watering);

        _ = Assert.ThrowsAsync<OwnershipException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Test]
    public void HandleAsync_WhenUserIsNotSpecieOwner_ThrowsOwnershipException()
    {
        var ownerId = Guid.NewGuid();
        var specieId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-001", specieId, ownerId);
        var specie = Specie.Create(specieId, "Monstera", Guid.NewGuid());

        var ctxMock = CreateContextMock([plant], [specie], []);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantActionScheduleCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new GetPlantActionScheduleCommand(plant.Id, PlantActionType.Watering);

        _ = Assert.ThrowsAsync<OwnershipException>(() => handler.HandleAsync(command, CancellationToken.None));
    }
}
