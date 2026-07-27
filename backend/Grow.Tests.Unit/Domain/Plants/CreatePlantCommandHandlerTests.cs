using Grow.Domain;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using Microsoft.EntityFrameworkCore;
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

        var dbSetMock = new Mock<DbSet<Plant>>();

        dbSetMock
            .As<IQueryable<Plant>>()
            .Setup(x => x.Provider)
            .Returns(plants.AsQueryable().Provider);

        dbSetMock
            .As<IQueryable<Plant>>()
            .Setup(x => x.Expression)
            .Returns(plants.AsQueryable().Expression);

        dbSetMock
            .As<IQueryable<Plant>>()
            .Setup(x => x.ElementType)
            .Returns(plants.AsQueryable().ElementType);

        dbSetMock
            .As<IQueryable<Plant>>()
            .Setup(x => x.GetEnumerator())
            .Returns(plants.AsQueryable().GetEnumerator);

        var contextMock = new Mock<IDatabaseContext>();

        contextMock
            .Setup(x => x.Plants)
            .Returns(dbSetMock.Object);

        var handler = new CreatePlantCommandHandler(contextMock.Object);

        var command = new CreatePlantCommand(
            Guid.NewGuid(),
               "plant-01",
               Guid.NewGuid());

        Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        contextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WhenCustomIdIsUnique_AddsPlantAndCallsSaveChanges()
    {
        var plants = new List<Plant>();

        var dbSetMock = new Mock<DbSet<Plant>>();

        dbSetMock
            .As<IQueryable<Plant>>()
            .Setup(x => x.Provider)
            .Returns(plants.AsQueryable().Provider);

        dbSetMock
            .As<IQueryable<Plant>>()
            .Setup(x => x.Expression)
            .Returns(plants.AsQueryable().Expression);

        dbSetMock
            .As<IQueryable<Plant>>()
            .Setup(x => x.ElementType)
            .Returns(plants.AsQueryable().ElementType);

        dbSetMock
            .As<IQueryable<Plant>>()
            .Setup(x => x.GetEnumerator())
            .Returns(plants.AsQueryable().GetEnumerator);

        var contextMock = new Mock<IDatabaseContext>();

        contextMock
            .Setup(x => x.Plants)
            .Returns(dbSetMock.Object);

        contextMock
           .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        var handler = new CreatePlantCommandHandler(contextMock.Object);

        var command = new CreatePlantCommand(
            Guid.NewGuid(),
               "plant-01",
               Guid.NewGuid());

        await handler.HandleAsync(command, CancellationToken.None);

        dbSetMock.Verify(
            x => x.Add(It.Is<Plant>(p =>
                p.Id == command.Id &&
                p.CustomId == command.CustomId &&
                p.SpecieId == command.SpecieId)),
            Times.Once);

        contextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
