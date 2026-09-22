using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using MockQueryable.Moq;
using Moq;

namespace Grow.Tests.Unit.Domain.Plants.Handlers;

[TestFixture]
public class GetPlantGroupsQueryHandlerTests
{
    public static Mock<IDatabaseContext> CreateContextMock(PlantGroup[] plantGroups)
    {
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.PlantGroups).Returns(plantGroups.ToList().BuildMockDbSet().Object);
        return ctxMock;
    }

    private static Mock<IAuthUserSessionProvider> CreateUserSessionProviderMock(Guid userId)
    {
        var userSessionProviderMock = new Mock<IAuthUserSessionProvider>();
        _ = userSessionProviderMock.Setup(x => x.Get()).Returns(new AuthUser(userId, true));
        return userSessionProviderMock;
    }

    [Test]
    public async Task HandleAsync_WhenGroupsExist_ReturnsAllUserGroups()
    {
        var ownerId = Guid.NewGuid();
        var region = PlantGroup.CreateRegion(Guid.NewGuid(), "Balcony", ownerId);
        var workGroup = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Peppers", ownerId);

        var ctxMock = CreateContextMock([region, workGroup]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantGroupsQueryHandler(ctxMock.Object, userSessionProviderMock.Object);
        var query = new GetPlantGroupsQuery(0, 20, null);

        var result = await handler.HandleAsync(query, CancellationToken.None);

        Assert.That(result.PlantGroups.Select(x => x.Id), Is.EquivalentTo(new[] { region.Id, workGroup.Id }));
    }

    [Test]
    public async Task HandleAsync_WhenSearchNameIsProvided_ReturnsOnlyMatchingGroups()
    {
        var ownerId = Guid.NewGuid();
        var region = PlantGroup.CreateRegion(Guid.NewGuid(), "Balcony", ownerId);
        var workGroup = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Peppers", ownerId);

        var ctxMock = CreateContextMock([region, workGroup]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantGroupsQueryHandler(ctxMock.Object, userSessionProviderMock.Object);
        var query = new GetPlantGroupsQuery(0, 20, "Balc");

        var result = await handler.HandleAsync(query, CancellationToken.None);

        Assert.That(result.PlantGroups.Select(x => x.Id), Is.EquivalentTo(new[] { region.Id }));
    }

    [Test]
    public async Task HandleAsync_WhenLimitIsLessThanTotalGroups_ReturnsLimitedGroups()
    {
        var ownerId = Guid.NewGuid();
        var region = PlantGroup.CreateRegion(Guid.NewGuid(), "Balcony", ownerId);
        var workGroup = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Peppers", ownerId);

        var ctxMock = CreateContextMock([region, workGroup]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantGroupsQueryHandler(ctxMock.Object, userSessionProviderMock.Object);
        var query = new GetPlantGroupsQuery(0, 1, null);

        var result = await handler.HandleAsync(query, CancellationToken.None);

        Assert.That(result.PlantGroups.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task HandleAsync_WhenFromSkipsGroups_ReturnsRemainingGroups()
    {
        var ownerId = Guid.NewGuid();
        var region = PlantGroup.CreateRegion(Guid.NewGuid(), "Balcony", ownerId);
        var workGroup = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Peppers", ownerId);

        var ctxMock = CreateContextMock([region, workGroup]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantGroupsQueryHandler(ctxMock.Object, userSessionProviderMock.Object);
        var query = new GetPlantGroupsQuery(1, 20, null);

        var result = await handler.HandleAsync(query, CancellationToken.None);

        Assert.That(result.PlantGroups.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task HandleAsync_WhenGroupBelongsToAnotherUser_DoesNotReturnGroup()
    {
        var ownerId = Guid.NewGuid();
        var otherOwnerId = Guid.NewGuid();
        var ownedGroup = PlantGroup.CreateRegion(Guid.NewGuid(), "Balcony", ownerId);
        var otherUsersGroup = PlantGroup.CreateRegion(Guid.NewGuid(), "Greenhouse", otherOwnerId);

        var ctxMock = CreateContextMock([ownedGroup, otherUsersGroup]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantGroupsQueryHandler(ctxMock.Object, userSessionProviderMock.Object);
        var query = new GetPlantGroupsQuery(0, 20, null);

        var result = await handler.HandleAsync(query, CancellationToken.None);

        Assert.That(result.PlantGroups.Select(x => x.Id), Is.EquivalentTo(new[] { ownedGroup.Id }));
    }

    [Test]
    public async Task HandleAsync_WhenGroupHasMembers_ReturnsGroupWithMemberships()
    {
        var ownerId = Guid.NewGuid();
        var plantId = Guid.NewGuid();
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Peppers", ownerId);
        group.AddPlant(plantId);

        var ctxMock = CreateContextMock([group]);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new GetPlantGroupsQueryHandler(ctxMock.Object, userSessionProviderMock.Object);
        var query = new GetPlantGroupsQuery(0, 20, null);

        var result = await handler.HandleAsync(query, CancellationToken.None);

        Assert.That(result.PlantGroups.Single().PlantGroupMemberships.Select(x => x.PlantId), Does.Contain(plantId));
    }
}
