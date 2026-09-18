using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using MockQueryable.Moq;
using Moq;
using System.Reflection;

namespace Grow.Tests.Unit.Domain.Plants.Handlers;

[TestFixture]
public class RemovePlantFromGroupCommandHandlerTests
{
    public static Mock<IDatabaseContext> CreateContextMock(Plant[]? plants = null, PlantGroup[]? plantGroups = null)
    {
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Plants).Returns((plants ?? []).ToList().BuildMockDbSet().Object);
        _ = ctxMock.Setup(c => c.PlantGroups).Returns((plantGroups ?? []).ToList().BuildMockDbSet().Object);
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
    public async Task HandleAsync_WhenGroupAndPlantExists_RemovesPlantFromGroupAndSaveChanges()
    {
        var ownerId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-02", Guid.NewGuid(), ownerId);
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony", ownerId);
        group.AddPlant(plant.Id);

        var ctxMock = CreateContextMock(plants: [plant], plantGroups: [group]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new RemovePlantFromGroupCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new RemovePlantFromGroupCommand(group.Id, plant.Id);

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(group.PlantGroupMemberships, Is.Empty);
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task HandleAsync_WhenPlantDoesNotExist_ThrowsExceptionAndDoesNotSave()
    {
        var ownerId = Guid.NewGuid();
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony", ownerId);
        var ctxMock = CreateContextMock(plantGroups: [group]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new RemovePlantFromGroupCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new RemovePlantFromGroupCommand(group.Id, Guid.NewGuid());

        var thrown = Assert.CatchAsync(() => handler.HandleAsync(command, CancellationToken.None))!;
        var actual = thrown is TargetInvocationException { InnerException: { } inner } ? inner : thrown;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual, Is.InstanceOf<InvalidOperationException>());
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task HandleAsync_WhenGroupDoesNotExist_ThrowsExceptionAndDoesNotSave()
    {
        var ownerId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-02", Guid.NewGuid(), ownerId);
        var ctxMock = CreateContextMock(plants: [plant]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new RemovePlantFromGroupCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new RemovePlantFromGroupCommand(Guid.NewGuid(), plant.Id);

        var thrown = Assert.CatchAsync(() => handler.HandleAsync(command, CancellationToken.None))!;
        var actual = thrown is TargetInvocationException { InnerException: { } inner } ? inner : thrown;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual, Is.InstanceOf<InvalidOperationException>());
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public void HandleAsync_WhenUserIsNotGroupOwner_ThrowsOwnershipExceptionAndDoesNotSave()
    {
        var plant = Plant.Create(Guid.NewGuid(), "monstera-02", Guid.NewGuid(), Guid.NewGuid());
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony", Guid.NewGuid());
        group.AddPlant(plant.Id);
        var ctxMock = CreateContextMock(plants: [plant], plantGroups: [group]);
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());

        var handler = new RemovePlantFromGroupCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new RemovePlantFromGroupCommand(group.Id, plant.Id);

        _ = Assert.ThrowsAsync<OwnershipException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void HandleAsync_WhenUserIsNotPlantOwner_ThrowsOwnershipExceptionAndDoesNotSave()
    {
        var ownerId = Guid.NewGuid();
        var plant = Plant.Create(Guid.NewGuid(), "monstera-02", Guid.NewGuid(), Guid.NewGuid());
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony", ownerId);
        group.AddPlant(plant.Id);
        var ctxMock = CreateContextMock(plants: [plant], plantGroups: [group]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new RemovePlantFromGroupCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new RemovePlantFromGroupCommand(group.Id, plant.Id);

        _ = Assert.ThrowsAsync<OwnershipException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
