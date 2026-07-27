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
    [Test]
    public async Task HandleAsync_WhenCustomIdAlreadyExists_ThrowsArgumentExceptionAndDoesNotSave()
    {
        var plants = new List<Plant>
        {
            Plant.Create(
                Guid.NewGuid(),
                "plant-01",
                Guid.NewGuid())
        };

        var plantsDbSet = plants.BuildMockDbSet();
        var ctxMock = new Mock<IDatabaseContext>();
        ctxMock.Setup(c => c.Plants).Returns(plantsDbSet.Object);
        ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreatePlantCommandHandler(ctxMock.Object);

        var command = new CreatePlantCommand(
            Guid.NewGuid(),
               "plant-01",
               Guid.NewGuid());

        Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WhenCustomIdIsUnique_AddsPlantAndCallsSaveChanges()
    {
        var plants = new List<Plant>();

        var plantsDbSet = plants.BuildMockDbSet();
        var ctxMock = new Mock<IDatabaseContext>();
        ctxMock.Setup(c => c.Plants).Returns(plantsDbSet.Object);
        ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreatePlantCommandHandler(ctxMock.Object);

        var command = new CreatePlantCommand(
            Guid.NewGuid(),
               "plant-01",
               Guid.NewGuid());

        await handler.HandleAsync(command, CancellationToken.None);

        plantsDbSet.Verify(
            x => x.Add(It.Is<Plant>(p =>
                p.Id == command.Id &&
                p.CustomId == command.CustomId &&
                p.SpecieId == command.SpecieId)),
            Times.Once);

        ctxMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
