namespace Grow.Domain.Users.Commons;

public static class OwnableExtensions
{
    extension(IOwnable ownable)
    {
        public bool CheckOwnership(Guid userId) => ownable.OwnerId == userId;

        public void ThrowIfNotOwner(Guid userId)
        {
            if (ownable.CheckOwnership(userId) == false)
            {
                throw new OwnershipException(ownable.OwnerId, userId);
            }
        }
    }
}
