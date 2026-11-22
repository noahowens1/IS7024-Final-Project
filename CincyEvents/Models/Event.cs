using System.ComponentModel.DataAnnotations;

namespace CincyEvents.Models;

public sealed class Event
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Category { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime? EndAtUtc { get; set; }

    [StringLength(100)]
    public string? TimeZoneId { get; set; }

    [StringLength(200)]
    public string? Venue { get; set; }

    public decimal? PriceMin { get; set; }

    public decimal? PriceMax { get; set; }

    [StringLength(2048)]
    [HttpUrl]
    public string? TicketUrl { get; set; }

    [Required]
    [StringLength(100)]
    public string SourceName { get; set; } = string.Empty;

    public bool IsSample { get; set; }

    public ICollection<SavedEvent> SavedEvents { get; set; } = new List<SavedEvent>();
}
