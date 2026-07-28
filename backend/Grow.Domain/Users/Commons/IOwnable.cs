using System;
using System.Collections.Generic;
using System.Text;

namespace Grow.Domain.Users.Commons;

public interface IOwnerable
{
    Guid OwnerId { get; }
}