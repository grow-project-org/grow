namespace Grow.Domain.Commons.Ownership;

public interface IOwnable
{
    Guid OwnerId { get; }
}