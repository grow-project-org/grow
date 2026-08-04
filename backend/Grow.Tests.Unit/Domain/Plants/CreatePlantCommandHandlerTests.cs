using Grow.Domain;
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

    [Test]
    public void HandleAsync_WhenSpecieDoesNotExist_ThrowsArgumentExceptionAndDoesNotSave()
    {
        var (ctxMock, _) = CreateContextMock([]);
        var handler = new CreatePlantCommandHandler(ctxMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", Guid.NewGuid());

        _ = Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void HandleAsync_WhenCustomIdAlreadyExists_ThrowsArgumentExceptionAndDoesNotSave()
    {
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa");
        var (ctxMock, _) = CreateContextMock([specie], Plant.Create(Guid.NewGuid(), "plant-01", specie.Id));
        var handler = new CreatePlantCommandHandler(ctxMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", specie.Id);

        _ = Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WhenSpecieExistsAndCustomIdIsUnique_AddsPlantAndCallsSaveChanges()
    {
        var specie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa");
        var (ctxMock, plantsDbSet) = CreateContextMock([specie]);
        var handler = new CreatePlantCommandHandler(ctxMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", specie.Id);

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            plantsDbSet.Verify(
                x => x.Add(It.Is<Plant>(p =>
                    p.Id == command.Id &&
                    p.CustomId == command.CustomId &&
                    p.SpecieId == command.SpecieId)),
                Times.Once);

            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
