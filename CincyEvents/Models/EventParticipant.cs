namespace CincyEvents.Models;

public class EventParticipant
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public bool IsOrganizer { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
