using System;
using System.Collections.Generic;
using System.Text;
using Grow.Domain.Users;

namespace Grow.Tests.Unit.Domain.User;

public class UserTests
{
    [Test]
    public void CreateUser_WithValidData_ShouldSuccess()
    {
        var userId = Guid.NewGuid();
        string email = "test@example.com";
        string username = "test_username";

        var user = global::Grow.Domain.Users.User.Create(userId, email, username); 

        Assert.That(user.Id, Is.EqualTo(userId));
        Assert.That(user.Email, Is.EqualTo(email));
        Assert.That(user.Username, Is.EqualTo(username));
    }

    [Test]
    public void CreateUser_WithEmptyId_ShouldThrowArgumentException()
    {
        var emptyId = Guid.Empty;
        string email = "test@example.com";
        string username = "test_username";

        var exception = Assert.Throws<ArgumentException>(() =>
             global::Grow.Domain.Users.User.Create(emptyId, email, username));

        Assert.That(exception.Message, Does.Contain("User ID cannot be empty"));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("invalid-email")]
    public void CreateUser_WithInvalidEmail_ShouldThrowArgumentExceptiuon(string email)
    {
        var userId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => global::Grow.Domain.Users.User.Create(userId, email, "testUsername"));
    }

    [TestCase("")]
    [TestCase("ab")]
    public void CreateUser_WithInvalidUsername_ShouldThrowArgumentExceptiuon(string username)
    {
        var userId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => global::Grow.Domain.Users.User.Create(userId, "test@example.com", username));
    }
}
