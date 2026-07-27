using Grow.Domain;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using Grow.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Grow.Tests.Unit.Domain.Plants;

[TestFixture]
public class CreatePlantCommandHandlerTests
{
    private DbContextOptions<DatabaseContext> _dbContextOptions;

    [SetUp]
    public void SetUp()
    {
        this._dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task HandleAsync_WhenCustomIdAlreadyExists_ThrowsArgumentExceptionAndDoesNotSave()
    {
        await using var dbContext = new DatabaseContext(this._dbContextOptions);
        dbContext.Plants.Add(Plant.Create(Guid.NewGuid(), "plant-01", Guid.NewGuid()));
        await dbContext.SaveChangesAsync();

        var contextMock = new Mock<IDatabaseContext>();
        contextMock.Setup(c => c.Plants).Returns(dbContext.Plants);

        var handler = new CreatePlantCommandHandler(contextMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-01", Guid.NewGuid());

        Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WhenCustomIdIsUnique_AddsPlantAndCallsSaveChanges()
    {
        await using var dbContext = new DatabaseContext(this._dbContextOptions);
        var contextMock = new Mock<IDatabaseContext>();
        contextMock.Setup(c => c.Plants).Returns(dbContext.Plants);
        contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreatePlantCommandHandler(contextMock.Object);
        var command = new CreatePlantCommand(Guid.NewGuid(), "plant-02", Guid.NewGuid());

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(dbContext.Plants.Local.Any(p => p.Id == command.Id && p.CustomId == "plant-02"), Is.True);
        contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
