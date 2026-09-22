using Grow.Domain.Commons;
using Grow.Domain.Species;
using Grow.Domain.Species.Handlers;
using MockQueryable.Moq;
using Moq;

namespace Grow.Tests.Unit.Domain.Species.Handlers;

[TestFixture]
public class GetSpeciesQueryHandlerTests
{
    private static Mock<IDatabaseContext> CreateContextMock(params Specie[] species)
    {
        var ctxMock = new Mock<IDatabaseContext>();
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
    public async Task HandleAsync_WhenNoSpeciesExist_ReturnsEmptyResult()
    {
        var ctxMock = CreateContextMock();
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new GetSpeciesQueryHandler(ctxMock.Object, userSessionProviderMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(0, 10, null), CancellationToken.None);

        Assert.That(result.Species, Is.Empty);
    }

    [Test]
    public async Task HandleAsync_WhenSpeciesExist_ReturnsOwnAndPublicSpecies()
    {
        var ownerId = Guid.NewGuid();
        var ownSpecie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var publicSpecie = Specie.Create(Guid.NewGuid(), "Boston Fern", Guid.Empty);
        var otherUserSpecie = Specie.Create(Guid.NewGuid(), "Cactus", Guid.NewGuid());
        var ctxMock = CreateContextMock(ownSpecie, publicSpecie, otherUserSpecie);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new GetSpeciesQueryHandler(ctxMock.Object, userSessionProviderMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(0, 10, null), CancellationToken.None);

        Assert.That(result.Species, Is.EquivalentTo(new[] { ownSpecie, publicSpecie }));
    }

    [Test]
    public async Task HandleAsync_WhenOnlyOtherUsersSpeciesExist_ReturnsEmptyResult()
    {
        var ctxMock = CreateContextMock(Specie.Create(Guid.NewGuid(), "Cactus", Guid.NewGuid()));
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new GetSpeciesQueryHandler(ctxMock.Object, userSessionProviderMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(0, 10, null), CancellationToken.None);

        Assert.That(result.Species, Is.Empty);
    }

    [Test]
    public async Task HandleAsync_WhenTakeIsLessThanTotalSpecies_ReturnsLimitedResult()
    {
        var ownerId = Guid.NewGuid();
        var firstSpecie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var secondSpecie = Specie.Create(Guid.NewGuid(), "Boston Fern", ownerId);
        var thirdSpecie = Specie.Create(Guid.NewGuid(), "Cactus", ownerId);
        var ctxMock = CreateContextMock(firstSpecie, secondSpecie, thirdSpecie);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new GetSpeciesQueryHandler(ctxMock.Object, userSessionProviderMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(0, 2, null), CancellationToken.None);

        Assert.That(result.Species, Has.Exactly(2).Items);
    }

    [Test]
    public async Task HandleAsync_WhenSkipIsGreaterThanZero_SkipsFirstRecords()
    {
        var ownerId = Guid.NewGuid();
        var firstSpecie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var secondSpecie = Specie.Create(Guid.NewGuid(), "Boston Fern", ownerId);
        var ctxMock = CreateContextMock(firstSpecie, secondSpecie);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new GetSpeciesQueryHandler(ctxMock.Object, userSessionProviderMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(1, 10, null), CancellationToken.None);

        Assert.That(result.Species, Is.EquivalentTo(new[] { secondSpecie }));
    }

    [Test]
    public async Task HandleAsync_WhenSearchNameIsProvided_ReturnsOnlyMatchingSpecies()
    {
        var ownerId = Guid.NewGuid();
        var matchingSpecie = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", ownerId);
        var nonMatchingSpecie = Specie.Create(Guid.NewGuid(), "Boston Fern", ownerId);
        var ctxMock = CreateContextMock(matchingSpecie, nonMatchingSpecie);
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new GetSpeciesQueryHandler(ctxMock.Object, userSessionProviderMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(0, 10, "Monstera"), CancellationToken.None);

        Assert.That(result.Species, Is.EquivalentTo(new[] { matchingSpecie }));
    }
}
