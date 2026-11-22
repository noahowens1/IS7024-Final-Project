using System.Security.Claims;
using CincyEvents.Data;
using CincyEvents.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CincyEvents.Pages.Chat;

[Authorize]
public class IndexModel(ApplicationDbContext db) : PageModel
{
    public Event Event { get; private set; } = null!;
    public IList<ChatMessage> Messages { get; private set; } = [];
    public IList<Participant> Participants { get; private set; } = [];
    public string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> OnGetAsync(int eventId, string? recipientId)
    {
        if (!await IsParticipant(eventId))
            return Forbid();

        Event = await db.Events.SingleAsync(item => item.Id == eventId);
        Participants = await db.EventParticipants
            .Where(item => item.EventId == eventId && item.UserId != CurrentUserId)
            .Select(item => new Participant(item.UserId, item.User.DisplayName ?? item.User.Email!))
            .ToListAsync();

        var conversation = await FindConversation(eventId, recipientId);
        if (conversation is not null)
        {
            Messages = await db.ChatMessages
                .Where(message => message.ConversationId == conversation.Id)
                .Include(message => message.Sender)
                .OrderBy(message => message.SentAt)
                .ToListAsync();
        }

        ViewData["RecipientId"] = recipientId;
        return Page();
    }

    private Task<bool> IsParticipant(int eventId) => db.EventParticipants.AnyAsync(item =>
        item.EventId == eventId && item.UserId == CurrentUserId);

    private Task<ChatConversation?> FindConversation(int eventId, string? recipientId)
    {
        if (string.IsNullOrWhiteSpace(recipientId))
            return db.ChatConversations.SingleOrDefaultAsync(item => item.EventId == eventId && item.IsGroup);

        var firstUserId = string.CompareOrdinal(CurrentUserId, recipientId) < 0 ? CurrentUserId : recipientId;
        var secondUserId = firstUserId == CurrentUserId ? recipientId : CurrentUserId;
        return db.ChatConversations.SingleOrDefaultAsync(item =>
            item.EventId == eventId && !item.IsGroup &&
            item.FirstUserId == firstUserId && item.SecondUserId == secondUserId);
    }

    public sealed record Participant(string Id, string Name);
}
