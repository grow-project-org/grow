using DomainUser = Grow.Domain.Users.User;

namespace Grow.Tests.Unit.Domain.User;

[TestFixture]
public class UserTests
{
    [Test]
    public void CreateUser_WithValidData_ShouldSuccess()
    {
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var username = "test_username";

        var user = DomainUser.Create(userId, email, username);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.Id, Is.EqualTo(userId));
            Assert.That(user.Email, Is.EqualTo(email));
            Assert.That(user.Username, Is.EqualTo(username));
        }
    }

    [Test]
    public void CreateUser_WithEmptyId_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            DomainUser.Create(Guid.Empty, "test@example.com", "test_username"))!;

        Assert.That(exception.Message, Does.Contain("User ID cannot be empty"));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("invalid-email")]
    public void CreateUser_WithInvalidEmail_ShouldThrowArgumentException(string email)
        => Assert.Throws<ArgumentException>(() => DomainUser.Create(Guid.NewGuid(), email, "testUsername"));

    [TestCase("")]
    [TestCase("ab")]
    public void CreateUser_WithInvalidUsername_ShouldThrowArgumentException(string username)
        => Assert.Throws<ArgumentException>(() => DomainUser.Create(Guid.NewGuid(), "test@example.com", username));
}
