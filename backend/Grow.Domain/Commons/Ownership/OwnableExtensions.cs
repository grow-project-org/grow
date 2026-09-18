namespace Grow.Domain.Commons.Ownership;

public static class OwnableExtensions
{
    extension(IOwnable ownable)
    {
        public bool CheckOwnership(AuthUser user) => ownable.CheckOwnership(user.Id);
        public bool CheckOwnership(Guid userId) => ownable.OwnerId == userId;

        public void ThrowIfNotOwner(AuthUser user) => ownable.ThrowIfNotOwner(user.Id);
        public void ThrowIfNotOwner(Guid userId)
        {
            if (ownable.CheckOwnership(userId) == false)
            {
                throw new OwnershipException(ownable.OwnerId, userId);
            }
        }
    }
}
