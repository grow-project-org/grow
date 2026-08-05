using Grow.Domain;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using MockQueryable.Moq;
using Moq;
using System.Reflection;

namespace Grow.Tests.Unit.Domain.Plants.Handlers;

[TestFixture]
public class CreatePlantGroupCommandHandlerTests
{
    public static Mock<IDatabaseContext> CreateContextMock(PlantGroup[]? plantGroups = null)
    {
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.PlantGroups).Returns((plantGroups ?? []).ToList().BuildMockDbSet().Object);
        _ = ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return ctxMock;
    }

    [Test]
    public async Task HandleAsync_WhenCreatingRegion_CreatesRegionGroup()
    {
        var ctxMock = CreateContextMock();

        var handler = new CreatePlantGroupCommandHandler(ctxMock.Object);
        var command = new CreatePlantGroupCommand(Guid.NewGuid(), "Europe", GroupType.Region);

        await handler.HandleAsync(command, CancellationToken.None);

        ctxMock.Verify(x => x.PlantGroups.Add(It.Is<PlantGroup>(g => g.Name == "Europe" && g.Type == GroupType.Region)), Times.Once);
    }

    [Test]
    public async Task HandleAsync_WhenCreatingWorkGroup_CreatesWorkGroup()
    {
        var ctxMock = CreateContextMock();

        var handler = new CreatePlantGroupCommandHandler(ctxMock.Object);
        var command = new CreatePlantGroupCommand(Guid.NewGuid(), "Garden", GroupType.WorkGroup);

        await handler.HandleAsync(command, CancellationToken.None);

        ctxMock.Verify(x => x.PlantGroups.Add(It.Is<PlantGroup>(g => g.Name == "Garden" && g.Type == GroupType.WorkGroup)), Times.Once);
    }

    [Test]
    public async Task HandleAsync_WhenCreatingTemporaryGroup_CreatesTemporaryGroup()
    {
        var ctxMock = CreateContextMock();

        var handler = new CreatePlantGroupCommandHandler(ctxMock.Object);
        var command = new CreatePlantGroupCommand(Guid.NewGuid(), "Inspection", GroupType.TemporaryGroup);

        await handler.HandleAsync(command, CancellationToken.None);

        ctxMock.Verify(x => x.PlantGroups.Add(It.Is<PlantGroup>(g => g.Name == "Inspection" && g.Type == GroupType.TemporaryGroup)), Times.Once);
    }

    [Test]
    public async Task HandleAsync_WhenGroupNameAlreadyExists_ThrowsExceptionAndDoesNotSave()
    {
        var existingGroup = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony", Guid.NewGuid());

        var ctxMock = CreateContextMock(plantGroups: [existingGroup]);

        var handler = new CreatePlantGroupCommandHandler(ctxMock.Object);
        var command = new CreatePlantGroupCommand(Guid.NewGuid(), "Balcony", GroupType.WorkGroup);

        var thrown = Assert.CatchAsync(() => handler.HandleAsync(command, CancellationToken.None))!;
        var actual = thrown is TargetInvocationException { InnerException: { } inner } ? inner : thrown;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual, Is.InstanceOf<ArgumentException>());
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public async Task HandleAsync_WhenGroupTypeIsInvalid_ThrowsExceptionAndDoesNotSave()
    {
        var ctxMock = CreateContextMock();

        var handler = new CreatePlantGroupCommandHandler(ctxMock.Object);
        var command = new CreatePlantGroupCommand(Guid.NewGuid(), "Invalid Group", (GroupType)999);

        var thrown = Assert.CatchAsync(() => handler.HandleAsync(command, CancellationToken.None))!;
        var actual = thrown is TargetInvocationException { InnerException: { } inner } ? inner : thrown;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual, Is.InstanceOf<ArgumentOutOfRangeException>());
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [TestCase(GroupType.Region)]
    [TestCase(GroupType.WorkGroup)]
    [TestCase(GroupType.TemporaryGroup)]
    public async Task HandleAsync_WhenGroupTypeIsValid_CreatesPlantGroupAndSaveChanges(GroupType type)
    {
        var ctxMock = CreateContextMock();

        var handler = new CreatePlantGroupCommandHandler(ctxMock.Object);
        var command = new CreatePlantGroupCommand(Guid.NewGuid(), "Balcony", type);

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            ctxMock.Verify(x => x.PlantGroups.Add(It.IsAny<PlantGroup>()), Times.Once);
            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
