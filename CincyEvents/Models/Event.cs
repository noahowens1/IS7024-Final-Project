namespace CincyEvents.Models;

public class Event
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public string? Source { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartsAt { get; set; }
    public ICollection<EventParticipant> Participants { get; set; } = [];
    public ICollection<ChatConversation> Conversations { get; set; } = [];
}
