using Grow.Domain.Commons;
using Grow.Domain.Species;
using Grow.Domain.Species.Handlers;
using MockQueryable.Moq;
using Moq;

namespace Grow.Tests.Unit.Domain.Species.Handlers;

[TestFixture]
public class CreateSpecieCommandHandlerTests
{
    private static (Mock<IDatabaseContext> Context, Mock<Microsoft.EntityFrameworkCore.DbSet<Specie>> Species) CreateContextMock(params Specie[] species)
    {
        var speciesDbSet = species.ToList().BuildMockDbSet();
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Species).Returns(speciesDbSet.Object);
        _ = ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (ctxMock, speciesDbSet);
    }

    private static Mock<IAuthUserSessionProvider> CreateUserSessionProviderMock(Guid userId)
    {
        var userSessionProviderMock = new Mock<IAuthUserSessionProvider>();
        _ = userSessionProviderMock.Setup(x => x.Get()).Returns(new AuthUser(userId, true));
        return userSessionProviderMock;
    }

    [Test]
    public async Task HandleAsync_AddsSpecieAndCallsSaveChanges()
    {
        var ownerId = Guid.NewGuid();
        var (ctxMock, speciesDbSet) = CreateContextMock();
        var userSessionProviderMock = CreateUserSessionProviderMock(ownerId);
        var handler = new CreateSpecieCommandHandler(ctxMock.Object, userSessionProviderMock.Object);
        var command = new CreateSpecieCommand(Guid.NewGuid(), "Monstera Deliciosa");

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            speciesDbSet.Verify(
                x => x.AddAsync(
                    It.Is<Specie>(s => s.Id == command.Id && s.Name == command.Name && s.OwnerId == ownerId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
