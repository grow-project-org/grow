using System;
using System.Collections.Generic;
using System.Text;

namespace Grow.Domain.Users;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }

    public static User Create(Guid id, string email, string username)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(id));
        }

        ValidateUsername(username);
        ValidateEmail(email);

        return new User(id, email, username);
    }

    private User(Guid id, string email, string username)
    {
        this.Id = id;
        this.Email = email;
        this.Username = username;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty or whitespace.", nameof(email));
        }

        if (!email.Contains('@'))
        {
            throw new ArgumentException("Invalid email format", nameof(email));
        }
    }

    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty or whitespace.", nameof(username));
        }

        if (username.Length < 3)
        {
            throw new ArgumentException("Username must be at least 3 characters long", nameof(username));
        }
    }
}
