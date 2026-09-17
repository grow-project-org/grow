using Grow.Domain;
using Grow.Domain.Users.Handlers;
using MockQueryable.Moq;
using Moq;
using DomainUser = Grow.Domain.Users.User;

namespace Grow.Tests.Unit.Domain.Users.Handlers;

[TestFixture]
public class CreateUserCommandHandlerTests
{
    private static (Mock<IDatabaseContext> Context, Mock<Microsoft.EntityFrameworkCore.DbSet<DomainUser>> Users) CreateContextMock(params DomainUser[] users)
    {
        var usersDbSet = users.ToList().BuildMockDbSet();
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Users).Returns(usersDbSet.Object);
        _ = ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (ctxMock, usersDbSet);
    }

    [Test]
    public async Task HandleAsync_AddsUserAndCallsSaveChanges()
    {
        var (ctxMock, usersDbSet) = CreateContextMock();
        var handler = new CreateUserCommandHandler(ctxMock.Object);
        var command = new CreateUserCommand(Guid.NewGuid(), "test@example.com", "test_username");

        await handler.HandleAsync(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            usersDbSet.Verify(
                x => x.AddAsync(
                    It.Is<DomainUser>(u => u.Id == command.Id && u.Email == command.Email && u.Username == command.Username),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public void HandleAsync_WhenEmailIsInvalid_ThrowsArgumentExceptionAndDoesNotSave()
    {
        var (ctxMock, _) = CreateContextMock();
        var handler = new CreateUserCommandHandler(ctxMock.Object);
        var command = new CreateUserCommand(Guid.NewGuid(), "invalid-email", "test_username");

        _ = Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void HandleAsync_WhenUsernameIsTooShort_ThrowsArgumentExceptionAndDoesNotSave()
    {
        var (ctxMock, _) = CreateContextMock();
        var handler = new CreateUserCommandHandler(ctxMock.Object);
        var command = new CreateUserCommand(Guid.NewGuid(), "test@example.com", "ab");

        _ = Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command, CancellationToken.None));

        ctxMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
