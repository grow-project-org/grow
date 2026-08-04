using Grow.Domain;
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

    [Test]
    public async Task HandleAsync_WhenNoSpeciesExist_ReturnsEmptyResult()
    {
        var ctxMock = CreateContextMock();
        var handler = new GetSpeciesQueryHandler(ctxMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(), CancellationToken.None);

        Assert.That(result.Species, Is.Empty);
    }

    [Test]
    public async Task HandleAsync_WhenSpeciesExist_ReturnsAllSpecies()
    {
        var monstera = Specie.Create(Guid.NewGuid(), "Monstera Deliciosa");
        var fern = Specie.Create(Guid.NewGuid(), "Boston Fern");
        var ctxMock = CreateContextMock(monstera, fern);
        var handler = new GetSpeciesQueryHandler(ctxMock.Object);

        var result = await handler.HandleAsync(new GetSpeciesQuery(), CancellationToken.None);

        Assert.That(result.Species, Is.EquivalentTo(new[] { monstera, fern }));
    }
}
