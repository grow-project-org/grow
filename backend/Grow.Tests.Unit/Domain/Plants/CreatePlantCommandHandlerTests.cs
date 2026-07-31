using Grow.Domain;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Grow.Tests.Unit.Domain.Plants;

[TestFixture]
public class CreatePlantCommandHandlerTests
{
    private static (Mock<IDatabaseContext> Context, Mock<DbSet<Plant>> Plants) CreateContextMock(params Plant[] plants)
    {
        var plantsDbSet = plants.ToList().BuildMockDbSet();
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Plants).Returns(plantsDbSet.Object);
        _ = ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (ctxMock, plantsDbSet);
    }

    [Test]
    public void HandleAsync_WhenCustomIdAlreadyExists_ThrowsArgumentExceptionAndDoesNotSave()
    {
        var (ctxMock, _) = CreateContextMock(Plant.Create(Guid.NewGuid(), "plant-01", Guid.NewGuid()));
        var handler = new CreatePlantCommandHandler(ctxMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", Guid.NewGuid());

        _ = Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WhenCustomIdIsUnique_AddsPlantAndCallsSaveChanges()
    {
        var (ctxMock, plantsDbSet) = CreateContextMock();
        var handler = new CreatePlantCommandHandler(ctxMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", Guid.NewGuid());

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
