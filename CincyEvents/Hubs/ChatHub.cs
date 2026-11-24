using System.Security.Claims;
using CincyEvents.Data;
using CincyEvents.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CincyEvents.Hubs;

[Authorize]
public class ChatHub(ApplicationDbContext db) : Hub
{
    public async Task JoinEventChat(int eventId)
    {
        await EnsureParticipant(eventId);
        await Groups.AddToGroupAsync(Context.ConnectionId, EventGroup(eventId));
    }

    public async Task SendMessage(int eventId, string? recipientId, string content)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException("You must be signed in.");
        var trimmedContent = content.Trim();

        if (trimmedContent.Length is 0 or > 2000)
            throw new HubException("Messages must contain between 1 and 2,000 characters.");

        await EnsureParticipant(eventId);
        ChatConversation conversation;

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            conversation = await db.ChatConversations.SingleOrDefaultAsync(item =>
                item.EventId == eventId && item.IsGroup)
                ?? new ChatConversation { EventId = eventId, IsGroup = true };
        }
        else
        {
            await EnsureParticipant(eventId, recipientId);
            var firstUserId = string.CompareOrdinal(userId, recipientId) < 0 ? userId : recipientId;
            var secondUserId = firstUserId == userId ? recipientId : userId;
            conversation = await db.ChatConversations.SingleOrDefaultAsync(item =>
                item.EventId == eventId && !item.IsGroup &&
                item.FirstUserId == firstUserId && item.SecondUserId == secondUserId)
                ?? new ChatConversation
                {
                    EventId = eventId,
                    FirstUserId = firstUserId,
                    SecondUserId = secondUserId
                };
        }

        if (conversation.Id == 0)
            db.ChatConversations.Add(conversation);

        var message = new ChatMessage
        {
            Conversation = conversation,
            SenderId = userId,
            Content = trimmedContent,
            SentAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        await Clients.Group(EventGroup(eventId)).SendAsync(
            "ReceiveMessage", userId, Context.User?.Identity?.Name ?? "User", trimmedContent, message.SentAt);
    }

    private async Task EnsureParticipant(int eventId, string? userId = null)
    {
        userId ??= Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !await db.EventParticipants.AnyAsync(item =>
                item.EventId == eventId && item.UserId == userId))
            throw new HubException("You must be connected to this event to use its chat.");
    }

    private static string EventGroup(int eventId) => $"event-chat-{eventId}";
}
