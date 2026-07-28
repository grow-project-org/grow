using System;
using System.Collections.Generic;
using System.Text;

namespace Grow.Domain.User.Extension;

public static class OwnerableExtensions
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
