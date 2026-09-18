using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using MockQueryable.Moq;
using Moq;
using System.Reflection;

namespace Grow.Tests.Unit.Domain.Plants.Handlers;

[TestFixture]
public class AddPlantEventCommandHandlerTests
{
    private static Mock<IDatabaseContext> CreateContextMock(params Plant[] plants)
    {
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Plants).Returns(plants.ToList().BuildMockDbSet().Object);
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
    public async Task HandleAsync_WhenPlantExists_AddsEventAndCallsSaveChanges()
    {
        var ownerId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-01", Guid.NewGuid(), ownerId);
        var ctxMock = CreateContextMock(plant);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new AddPlantEventCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new AddPlantEventCommand(plant.Id, Guid.NewGuid(), PlantActionType.Watering, DateTime.UtcNow);

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(plant.Events, Has.Count.EqualTo(1));

        var @event = plant.Events.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(@event.Id, Is.EqualTo(command.ActionLogId));
            Assert.That(@event.Type, Is.EqualTo(command.Type));
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public void HandleAsync_WhenPlantDoesNotExist_ThrowsAndDoesNotSave()
    {
        var ctxMock = CreateContextMock();
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new AddPlantEventCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new AddPlantEventCommand(Guid.NewGuid(), Guid.NewGuid(), PlantActionType.Watering, DateTime.UtcNow);

        var thrown = Assert.CatchAsync(() => handler.HandleAsync(command, CancellationToken.None))!;
        var actual = thrown is TargetInvocationException { InnerException: { } inner } ? inner : thrown;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual, Is.InstanceOf<InvalidOperationException>());
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public void HandleAsync_WhenUserIsNotPlantOwner_ThrowsOwnershipExceptionAndDoesNotSave()
    {
        var plant = Plant.Create(Guid.NewGuid(), "monstera-01", Guid.NewGuid(), Guid.NewGuid());
        var ctxMock = CreateContextMock(plant);
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new AddPlantEventCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new AddPlantEventCommand(plant.Id, Guid.NewGuid(), PlantActionType.Watering, DateTime.UtcNow);

        _ = Assert.ThrowsAsync<OwnershipException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
