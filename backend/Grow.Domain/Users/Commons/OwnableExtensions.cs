using System;
using System.Collections.Generic;
using System.Text;

namespace Grow.Domain.Users.Commons;

public static class OwnableExtensions
{
    extension(IOwnerable ownerable)
    {
        public bool CheckOwnership(Guid userId) => ownerable.OwnerId == userId;

        public void ThrowIfNotOwner(Guid userId)
        {
            if (ownerable.CheckOwnership(userId) == false)
            {
                throw new OwnershipException(ownerable.OwnerId, userId);
            }
        }
    }
}
