using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Domain.Species;
using Grow.Domain.Species.Handlers;
using MockQueryable.Moq;
using Moq;

namespace Grow.Tests.Unit.Domain.Species.Handlers;

[TestFixture]
public class UpdateSpecieIntervalCommandHandlerTests
{
    private static Mock<IDatabaseContext> CreateContextMock(params Specie[] species)
    {
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Species).Returns(species.ToList().BuildMockDbSet().Object);
        _ = ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return ctxMock;
    }

    private static Mock<IAuthUserSessionProvider> CreateUserSessionProviderMock(Guid userId)
    {
        var userSessionProviderMock = new Mock<IAuthUserSessionProvider>();
        _ = userSessionProviderMock.Setup(x => x.Get()).Returns(new AuthUser(userId, true));
        return userSessionProviderMock;
    }

    [Test]
    public void HandleAsync_WhenSpecieDoesNotExist_ThrowsArgumentExceptionAndDoesNotSave()
    {
        var ctxMock = CreateContextMock();
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new UpdateSpecieIntervalCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new UpdateSpecieIntervalCommand(Guid.NewGuid(), PlantActionType.Watering, TimeSpan.FromDays(7));

        _ = Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void HandleAsync_WhenUserIsNotSpecieOwner_ThrowsOwnershipExceptionAndDoesNotSave()
    {
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", Guid.NewGuid());
        var ctxMock = CreateContextMock(specie);
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new UpdateSpecieIntervalCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new UpdateSpecieIntervalCommand(specie.Id, PlantActionType.Watering, TimeSpan.FromDays(7));

        _ = Assert.ThrowsAsync<OwnershipException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WhenActionTypeIsWatering_SetsWateringIntervalAndSaveChanges()
    {
        var ownerId = Guid.NewGuid();
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var ctxMock = CreateContextMock(specie);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new UpdateSpecieIntervalCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var interval = TimeSpan.FromDays(7);
        var command = new UpdateSpecieIntervalCommand(specie.Id, PlantActionType.Watering, interval);

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(specie.Intervals[PlantActionType.Watering], Is.EqualTo(interval));
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task HandleAsync_WhenActionTypeIsFertilizing_SetsFertilizingIntervalAndSaveChanges()
    {
        var ownerId = Guid.NewGuid();
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var ctxMock = CreateContextMock(specie);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new UpdateSpecieIntervalCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var interval = TimeSpan.FromDays(30);
        var command = new UpdateSpecieIntervalCommand(specie.Id, PlantActionType.Fertilizing, interval);

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(specie.Intervals[PlantActionType.Fertilizing], Is.EqualTo(interval));
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task HandleAsync_CalledTwiceForSameActionType_UpdatesExistingInterval()
    {
        var ownerId = Guid.NewGuid();
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var ctxMock = CreateContextMock(specie);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new UpdateSpecieIntervalCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        await handler.HandleAsync(new UpdateSpecieIntervalCommand(specie.Id, PlantActionType.Watering, TimeSpan.FromDays(7)), CancellationToken.None);

        await handler.HandleAsync(new UpdateSpecieIntervalCommand(specie.Id, PlantActionType.Watering, TimeSpan.FromDays(3)), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(specie.Intervals[PlantActionType.Watering], Is.EqualTo(TimeSpan.FromDays(3)));
            Assert.That(specie.Intervals, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void HandleAsync_WhenActionTypeIsUnsupported_ThrowsNotImplementedExceptionAndDoesNotSave()
    {
        var ownerId = Guid.NewGuid();
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var ctxMock = CreateContextMock(specie);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new UpdateSpecieIntervalCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new UpdateSpecieIntervalCommand(specie.Id, (PlantActionType)999, TimeSpan.FromDays(7));

        _ = Assert.ThrowsAsync<NotImplementedException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
