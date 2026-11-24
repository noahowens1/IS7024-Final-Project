namespace CincyEvents.Models;

public class ChatConversation
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public bool IsGroup { get; set; }
    public string? FirstUserId { get; set; }
    public ApplicationUser? FirstUser { get; set; }
    public string? SecondUserId { get; set; }
    public ApplicationUser? SecondUser { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = [];
}
