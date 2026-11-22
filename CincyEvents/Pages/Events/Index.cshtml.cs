using CincyEvents.Data;
using CincyEvents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CincyEvents.Pages.Events;

public sealed class IndexModel(ApplicationDbContext dbContext) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<Event> UpcomingEvents { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        UpcomingEvents = await dbContext.Events
            .AsNoTracking()
            .Where(@event => @event.StartAtUtc > nowUtc)
            .OrderBy(@event => @event.StartAtUtc)
            .ToListAsync(cancellationToken);
    }
}
