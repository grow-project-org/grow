using Grow.Domain;
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

    [Test]
    public async Task HandleAsync_AddsSpecieAndCallsSaveChanges()
    {
        var (ctxMock, speciesDbSet) = CreateContextMock();
        var handler = new CreateSpecieCommandHandler(ctxMock.Object);
        var command = new CreateSpecieCommand(Guid.NewGuid(), "Monstera Deliciosa");

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            speciesDbSet.Verify(
                x => x.AddAsync(
                    It.Is<Specie>(s => s.Id == command.Id && s.Name == command.Name),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
