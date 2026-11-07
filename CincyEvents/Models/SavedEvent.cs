using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CincyEvents.Models;

public sealed class SavedEvent
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public IdentityUser User { get; set; } = null!;

    public int EventId { get; set; }

    [Required]
    public Event Event { get; set; } = null!;

    public DateTime SavedAtUtc { get; set; }
}
