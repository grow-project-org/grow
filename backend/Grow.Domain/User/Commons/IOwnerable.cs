using System;
using System.Collections.Generic;
using System.Text;

namespace Grow.Domain.User;

public interface IOwnerable
{
    Guid OwnerId { get; }
}