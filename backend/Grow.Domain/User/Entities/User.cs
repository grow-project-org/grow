using System;
using System.Collections.Generic;
using System.Text;

namespace Grow.Domain.User.Entities;

public class User
{
    public Guid OwnerId { get; private set; } = Guid.NewGuid();
    public String Email { get; private set; }
    public String Username { get; private set; }

    public User(String email, String username)
    {
        this.Username = username;
        this.Email = email;
    }
}
