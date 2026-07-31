using Grow.Domain;
using Grow.Domain.Commons;
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

    [Test]
    public async Task HandleAsync_WhenPlantExists_AddsEventAndCallsSaveChanges()
    {
        var plant = Plant.Create(Guid.NewGuid(), "monstera-01", Guid.NewGuid());
        var ctxMock = CreateContextMock(plant);
        var handler = new AddPlantEventCommandHandler(ctxMock.Object);
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
        var handler = new AddPlantEventCommandHandler(ctxMock.Object);
        var command = new AddPlantEventCommand(Guid.NewGuid(), Guid.NewGuid(), PlantActionType.Watering, DateTime.UtcNow);

        var thrown = Assert.CatchAsync(() => handler.HandleAsync(command, CancellationToken.None))!;
        var actual = thrown is TargetInvocationException { InnerException: { } inner } ? inner : thrown;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual, Is.InstanceOf<InvalidOperationException>());
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
