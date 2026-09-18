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

        var result = await handler.HandleAsync(new GetSpeciesQuery(), CancellationToken.None);

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

        var result = await handler.HandleAsync(new GetSpeciesQuery(), CancellationToken.None);

        Assert.That(result.Species, Is.EquivalentTo(new[] { ownSpecie, publicSpecie }));
    }

    [Test]
    public async Task HandleAsync_WhenOnlyOtherUsersSpeciesExist_ReturnsEmptyResult()
    {
        var ctxMock = CreateContextMock(Specie.Create(Guid.NewGuid(), "Cactus", Guid.NewGuid()));
        var userSessionProviderMock = CreateUserSessionProviderMock(Guid.NewGuid());
        var handler = new GetSpeciesQueryHandler(ctxMock.Object, userSessionProviderMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(), CancellationToken.None);

        Assert.That(result.Species, Is.Empty);
    }
}
