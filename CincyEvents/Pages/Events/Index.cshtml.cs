using System.Security.Claims;
using CincyEvents.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CincyEvents.Pages.Events;

[Authorize]
public class IndexModel(ApplicationDbContext db) : PageModel
{
    public IList<EventSummary> Events { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Events = await db.Events
            .Select(eventItem => new EventSummary
            {
                Id = eventItem.Id,
                Name = eventItem.Name,
                Description = eventItem.Description,
                StartsAt = eventItem.StartsAt,
                IsConnected = eventItem.Participants.Any(participant => participant.UserId == userId)
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostJoinAsync(int eventId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await db.EventParticipants.AnyAsync(item => item.EventId == eventId && item.UserId == userId))
        {
            db.EventParticipants.Add(new Models.EventParticipant { EventId = eventId, UserId = userId });
            await db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public sealed class EventSummary
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public DateTime StartsAt { get; init; }
        public bool IsConnected { get; init; }
    }
}
