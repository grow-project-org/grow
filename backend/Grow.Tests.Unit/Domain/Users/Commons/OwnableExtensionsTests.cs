using Grow.Domain.Commons.Ownership;

namespace Grow.Tests.Unit.Domain.Users.Commons;

[TestFixture]
public class OwnableExtensionsTests
{
    private sealed class FakeOwnable(Guid ownerId) : IOwnable
    {
        public Guid OwnerId { get; } = ownerId;
    }

    [Test]
    public void CheckOwnership_WhenUserIsOwner_ShouldReturnTrue()
    {
        var ownerId = Guid.NewGuid();
        IOwnable ownable = new FakeOwnable(ownerId);

        Assert.That(ownable.CheckOwnership(ownerId), Is.True);
    }

    [Test]
    public void CheckOwnership_WhenUserIsNotOwner_ShouldReturnFalse()
    {
        IOwnable ownable = new FakeOwnable(Guid.NewGuid());

        Assert.That(ownable.CheckOwnership(Guid.NewGuid()), Is.False);
    }

    [Test]
    public void ThrowIfNotOwner_WhenUserIsOwner_ShouldNotThrow()
    {
        var ownerId = Guid.NewGuid();
        IOwnable ownable = new FakeOwnable(ownerId);

        Assert.DoesNotThrow(() => ownable.ThrowIfNotOwner(ownerId));
    }

    [Test]
    public void ThrowIfNotOwner_WhenUserIsNotOwner_ShouldThrowOwnershipException()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        IOwnable ownable = new FakeOwnable(ownerId);

        var exception = Assert.Throws<OwnershipException>(() => ownable.ThrowIfNotOwner(otherUserId))!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.ObjectOwnerId, Is.EqualTo(ownerId));
            Assert.That(exception.ActionPerformer, Is.EqualTo(otherUserId));
        }
    }
}
