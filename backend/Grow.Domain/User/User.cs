using System;
using System.Collections.Generic;
using System.Text;

namespace Grow.Domain.User.Entities;

public class User
{
    public Guid OwnerId { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; }
    public string Username { get; private set; }

    public User(string email, string username)
    {
        this.Username = username;
        this.Email = email;
    }
}
