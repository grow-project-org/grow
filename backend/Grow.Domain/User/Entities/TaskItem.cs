using Grow.Domain.User.Extension;
using Grow.Domain.User.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Grow.Domain.User.Entities;

public class TaskItem : IOwnerable
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public Guid OwnerId { get; private set; }
    public bool IsCompleted { get; private set; }

    public TaskItem(string name, Guid ownerId)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Task name cannot be empty", nameof(name));
        }

        this.Name = name;
        this.OwnerId = ownerId;
    }

    public void Rename(string newName, Guid requestedBy)
    {
        this.ThrowIfNotOwner(requestedBy);

        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Task name cannot be empty", nameof(newName));
        }

        this.Name = newName;
    }

    public void Complete(Guid requestedBy)
    {
        this.ThrowIfNotOwner(requestedBy);
        this.IsCompleted = true;
    }

    public void ReOpen(Guid requestedBy)
    {
        this.ThrowIfNotOwner(requestedBy);
        this.IsCompleted = false;
    }
}
