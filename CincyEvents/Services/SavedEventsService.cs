using System.Security.Claims;
using CincyEvents.Data;
using CincyEvents.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CincyEvents.Services;

public enum SaveEventResult
{
    Saved,
    AlreadySaved,
    EventNotFound,
    EventUnavailable,
    InvalidEventId
}

public enum RemoveSavedEventResult
{
    Removed,
    NotFound
}

public sealed record SavedEventDetails(
    int SavedEventId,
    int EventId,
    string Title,
    string? Category,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    string? TimeZoneId,
    string? Venue,
    decimal? PriceMin,
    decimal? PriceMax,
    string? TicketUrl,
    bool IsSample);

public sealed class SavedEventsService(
    ApplicationDbContext dbContext,
    TimeProvider timeProvider)
{
    public async Task<SaveEventResult> SaveAsync(
        ClaimsPrincipal principal,
        int eventId,
        CancellationToken cancellationToken = default)
    {
        if (eventId <= 0)
        {
            return SaveEventResult.InvalidEventId;
        }

        var userId = GetUserId(principal);
        var @event = await dbContext.Events.SingleOrDefaultAsync(
            candidate => candidate.Id == eventId,
            cancellationToken);

        if (@event is null)
        {
            return SaveEventResult.EventNotFound;
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        if (@event.StartAtUtc <= nowUtc
            || @event.EndAtUtc is { } endAtUtc && endAtUtc <= nowUtc)
        {
            return SaveEventResult.EventUnavailable;
        }

        var alreadySaved = await dbContext.SavedEvents.AnyAsync(
            savedEvent => savedEvent.UserId == userId && savedEvent.EventId == eventId,
            cancellationToken);
        if (alreadySaved)
        {
            return SaveEventResult.AlreadySaved;
        }

        var savedEvent = new SavedEvent
        {
            UserId = userId,
            EventId = eventId,
            SavedAtUtc = nowUtc
        };
        dbContext.SavedEvents.Add(savedEvent);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return SaveEventResult.Saved;
        }
        catch (DbUpdateException exception) when (IsSavedEventUniqueConflict(exception))
        {
            dbContext.Entry(savedEvent).State = EntityState.Detached;
            return SaveEventResult.AlreadySaved;
        }
    }

    public async Task<IReadOnlyList<SavedEventDetails>> ListAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(principal);
        var savedEvents = await dbContext.SavedEvents
            .AsNoTracking()
            .Where(savedEvent => savedEvent.UserId == userId)
            .OrderBy(savedEvent => savedEvent.Event.StartAtUtc)
            .Select(savedEvent => new SavedEventDetails(
                savedEvent.Id,
                savedEvent.EventId,
                savedEvent.Event.Title,
                savedEvent.Event.Category,
                savedEvent.Event.StartAtUtc,
                savedEvent.Event.EndAtUtc,
                savedEvent.Event.TimeZoneId,
                savedEvent.Event.Venue,
                savedEvent.Event.PriceMin,
                savedEvent.Event.PriceMax,
                savedEvent.Event.TicketUrl,
                savedEvent.Event.IsSample))
            .ToListAsync(cancellationToken);

        return savedEvents;
    }

    public async Task<RemoveSavedEventResult> RemoveAsync(
        ClaimsPrincipal principal,
        int savedEventId,
        CancellationToken cancellationToken = default)
    {
        if (savedEventId <= 0)
        {
            return RemoveSavedEventResult.NotFound;
        }

        var userId = GetUserId(principal);
        var savedEvent = await dbContext.SavedEvents.SingleOrDefaultAsync(
            candidate => candidate.Id == savedEventId && candidate.UserId == userId,
            cancellationToken);
        if (savedEvent is null)
        {
            return RemoveSavedEventResult.NotFound;
        }

        dbContext.SavedEvents.Remove(savedEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RemoveSavedEventResult.Removed;
    }

    private static string GetUserId(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException(
                "The authenticated principal does not contain a user identifier.");
    }

    private static bool IsSavedEventUniqueConflict(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException
        {
            SqliteErrorCode: 19,
            SqliteExtendedErrorCode: 2067
        };
    }
}
