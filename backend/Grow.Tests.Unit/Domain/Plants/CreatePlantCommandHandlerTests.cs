using Grow.Domain.Commons;
using Grow.Domain.Commons.Ownership;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using Grow.Domain.Species;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Grow.Tests.Unit.Domain.Plants;

[TestFixture]
public class CreatePlantCommandHandlerTests
{
    private static (Mock<IDatabaseContext> Context, Mock<DbSet<Plant>> Plants) CreateContextMock(Specie[] species, params Plant[] plants)
    {
        var plantsDbSet = plants.ToList().BuildMockDbSet();
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Plants).Returns(plantsDbSet.Object);
        _ = ctxMock.Setup(c => c.Species).Returns(species.ToList().BuildMockDbSet().Object);
        _ = ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (ctxMock, plantsDbSet);
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
        var (ctxMock, _) = CreateContextMock([]);
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new CreatePlantCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", Guid.NewGuid());

        _ = Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void HandleAsync_WhenUserIsNotSpecieOwner_ThrowsOwnershipExceptionAndDoesNotSave()
    {
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", Guid.NewGuid());
        var (ctxMock, _) = CreateContextMock([specie]);
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new CreatePlantCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", specie.Id);

        _ = Assert.ThrowsAsync<OwnershipException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void HandleAsync_WhenCustomIdAlreadyExistsForUser_ThrowsArgumentExceptionAndDoesNotSave()
    {
        var ownerId = Guid.NewGuid();
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var (ctxMock, _) = CreateContextMock([specie], Plant.Create(Guid.NewGuid(), "plant-01", specie.Id, ownerId));
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new CreatePlantCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", specie.Id);

        _ = Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WhenCustomIdAlreadyExistsForAnotherUser_AddsPlantAndCallsSaveChanges()
    {
        var ownerId = Guid.NewGuid();
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var (ctxMock, plantsDbSet) = CreateContextMock([specie], Plant.Create(Guid.NewGuid(), "plant-01", specie.Id, Guid.NewGuid()));
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new CreatePlantCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", specie.Id);

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            plantsDbSet.Verify(x => x.Add(It.Is<Plant>(p => p.Id == command.Id && p.CustomId == command.CustomId)), Times.Once);
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task HandleAsync_WhenSpecieExistsAndCustomIdIsUnique_AddsPlantAndCallsSaveChanges()
    {
        var ownerId = Guid.NewGuid();
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var (ctxMock, plantsDbSet) = CreateContextMock([specie]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new CreatePlantCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", specie.Id);

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            plantsDbSet.Verify(
                x => x.Add(It.Is<Plant>(p =>
                    p.Id == command.Id &&
                    p.CustomId == command.CustomId &&
                    p.SpecieId == command.SpecieId &&
                    p.OwnerId == ownerId)),
                Times.Once);

            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
