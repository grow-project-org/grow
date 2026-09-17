using Grow.Domain;
using Grow.Domain.Users.Handlers;
using MockQueryable.Moq;
using Moq;
using DomainUser = Grow.Domain.Users.User;

namespace Grow.Tests.Unit.Domain.Users.Handlers;

[TestFixture]
public class GetUserByEmailQueryHandlerTests
{
    private static Mock<IDatabaseContext> CreateContextMock(params DomainUser[] users)
    {
        var ctxMock = new Mock<IDatabaseContext>();
        _ = ctxMock.Setup(c => c.Users).Returns(users.ToList().BuildMockDbSet().Object);
        return ctxMock;
    }

    [Test]
    public async Task HandleAsync_WhenUserWithEmailExists_ReturnsUser()
    {
        var user = DomainUser.Create(Guid.NewGuid(), "test@example.com", "test_username");
        var otherUser = DomainUser.Create(Guid.NewGuid(), "other@example.com", "other_username");
        var ctxMock = CreateContextMock(user, otherUser);
        var handler = new GetUserByEmailQueryHandler(ctxMock.Object);

        var result = await handler.HandleAsync(new GetUserByEmailQuery(user.Email), CancellationToken.None);

        Assert.That(result.User, Is.EqualTo(user));
    }

    [Test]
    public async Task HandleAsync_WhenUserWithEmailDoesNotExist_ReturnsNull()
    {
        var ctxMock = CreateContextMock();
        var handler = new GetUserByEmailQueryHandler(ctxMock.Object);

        var result = await handler.HandleAsync(new GetUserByEmailQuery("missing@example.com"), CancellationToken.None);

        Assert.That(result.User, Is.Null);
    }
}
