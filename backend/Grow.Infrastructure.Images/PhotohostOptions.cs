using System.ComponentModel.DataAnnotations;

namespace Grow.Infrastructure.Images;

public class PhotohostOptions
{
    [Required]
    [Url]
    public required Uri Address { get; init; }

    [Required]
    public required string ApiKey { get; init; }
}
