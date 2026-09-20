using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Plants.Handlers;
using Grow.Domain.Species;
using MockQueryable.Moq;
using Moq;

namespace Grow.Tests.Unit.Domain.Plants.Handlers;

[TestFixture]
public class SearchPlantsCommandHandlerTests
{
    public static Mock<IDatabaseContext> CreateContextMock(Plant[] plants, Specie[] species)
    {
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Plants).Returns(plants.ToList().BuildMockDbSet().Object);
        _ = ctxMock.Setup(c => c.Species).Returns(species.ToList().BuildMockDbSet().Object);
        return ctxMock;
    }

    private static Mock<IAuthUserSessionProvider> CreateUserSessionProviderMock(Guid userId)
    {
        var userSessionProviderMock = new Mock<IAuthUserSessionProvider>();
        _ = userSessionProviderMock.Setup(x => x.Get()).Returns(new AuthUser(userId, true));
        return userSessionProviderMock;
    }

    [Test]
    public async Task HandleAsync_WhenSearchTextMatchesCustomId_ReturnsMatchingPlants()
    {
        var ownerId = Guid.NewGuid();
        var specieId = Guid.NewGuid();

        var specie = Specie.Create(specieId, "Monstera", ownerId);
        var plantOne = Plant.Create(Guid.NewGuid(), "monstera-001", specieId, ownerId);
        var plantSecond = Plant.Create(Guid.NewGuid(), "bambus-001", specieId, ownerId);

        var ctxMock = CreateContextMock([plantOne, plantSecond], [specie]);

        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new SearchPlantsCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new SearchPlantsCommand("monstera");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.Plants.Count(), Is.EqualTo(1));
        Assert.That(result.Plants.Single(), Is.EqualTo(plantOne));
    }

    [Test]
    public async Task HandleAsync_WhenSearchTextMatchesSpecieName_ReturnsMatchingPlants()
    {
        var ownerId = Guid.NewGuid();
        var specieOneId = Guid.NewGuid();
        var specieTwoId = Guid.NewGuid();

        var specieOne = Specie.Create(specieOneId, "Bambus Bisseta", ownerId);
        var specieSecond = Specie.Create(specieTwoId, "Monstera", ownerId);

        var plantOne = Plant.Create(Guid.NewGuid(), "monstera-001", specieOneId, ownerId);
        var plantSecond = Plant.Create(Guid.NewGuid(), "bambus-001", specieTwoId, ownerId);

        var ctxMock = CreateContextMock([plantOne, plantSecond], [specieOne, specieSecond]);

        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new SearchPlantsCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new SearchPlantsCommand("Bisseta");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.Plants.Count(), Is.EqualTo(1));
        Assert.That(result.Plants.Single(), Is.EqualTo(plantOne));
    }

    [Test]
    public async Task HandleAsync_WhenSearchTextIsNull_ReturnsAllUserPlants()
    {
        var ownerId = Guid.NewGuid();
        var specieOneId = Guid.NewGuid();
        var specieTwoId = Guid.NewGuid();

        var specieOne = Specie.Create(specieOneId, "Bambus Bisseta", ownerId);
        var specieSecond = Specie.Create(specieTwoId, "Monstera", ownerId);

        var plantOne = Plant.Create(Guid.NewGuid(), "monstera-001", specieOneId, ownerId);
        var plantSecond = Plant.Create(Guid.NewGuid(), "bambus-001", specieTwoId, ownerId);

        var ctxMock = CreateContextMock([plantOne, plantSecond], [specieOne, specieSecond]);

        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new SearchPlantsCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new SearchPlantsCommand(null);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.Plants.Count(), Is.EqualTo(2));
        Assert.That(result.Plants, Does.Contain(plantOne));
        Assert.That(result.Plants, Does.Contain(plantSecond));
    }

    [Test]
    public async Task HandleAsync_WhenSearchTextDoesNotMatch_ReturnsEmpty()
    {
        var ownerId = Guid.NewGuid();
        var specieOneId = Guid.NewGuid();
        var specieTwoId = Guid.NewGuid();

        var specieOne = Specie.Create(specieOneId, "Bambus Bisseta", ownerId);
        var specieSecond = Specie.Create(specieTwoId, "Monstera", ownerId);

        var plantOne = Plant.Create(Guid.NewGuid(), "monstera-001", specieOneId, ownerId);
        var plantSecond = Plant.Create(Guid.NewGuid(), "bambus-001", specieTwoId, ownerId);

        var ctxMock = CreateContextMock([plantOne, plantSecond], [specieOne, specieSecond]);

        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);

        var handler = new SearchPlantsCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new SearchPlantsCommand("cactus");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.Plants, Is.Empty);
    }

    [Test]
    public async Task HandleAsync_WhenPlantBelongsToAnotherUser_DoesNotReturnPlant()
    {
        var ownerOneId = Guid.NewGuid();
        var ownerTwoId = Guid.NewGuid();

        var specieOneId = Guid.NewGuid();
        var specieTwoId = Guid.NewGuid();

        var specieOne = Specie.Create(specieOneId, "Bambus Bisseta", ownerOneId);
        var specieSecond = Specie.Create(specieTwoId, "Monstera", ownerOneId);

        var plantOne = Plant.Create(Guid.NewGuid(), "monstera-001", specieOneId, ownerOneId);
        var plantSecond = Plant.Create(Guid.NewGuid(), "bambus-001", specieTwoId, ownerTwoId);

        var ctxMock = CreateContextMock([plantOne, plantSecond], [specieOne, specieSecond]);

        var userSessionProviderMock = CreateUserSessionProviderMock(ownerOneId);

        var handler = new SearchPlantsCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new SearchPlantsCommand("bambus");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.Plants, Is.Empty);
    }
}
