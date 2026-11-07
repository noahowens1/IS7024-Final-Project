using CincyEvents.Models;
using CincyEvents.Services;
using CincyEvents.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CincyEvents.Tests.Services;

public sealed class SavedEventsDuplicateAndRemoveTests
{
    [Fact]
    public async Task SaveAsync_reports_duplicate_without_adding_another_relationship()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var dbContext = database.CreateDbContext();
        var user = await TestIdentityUser.CreateAsync(dbContext);
        var otherUser = await TestIdentityUser.CreateAsync(dbContext);
        var @event = new Event
        {
            Title = "Upcoming",
            StartAtUtc = DateTime.UtcNow.AddDays(1),
            SourceName = "Test"
        };
        dbContext.Events.Add(@event);
        await dbContext.SaveChangesAsync();
        var principal = TestIdentityUser.CreatePrincipal(user);
        var service = new SavedEventsService(dbContext, TimeProvider.System);

        Assert.Equal(SaveEventResult.Saved, await service.SaveAsync(principal, @event.Id));
        Assert.Equal(SaveEventResult.AlreadySaved, await service.SaveAsync(principal, @event.Id));

        var savedEvent = await dbContext.SavedEvents.SingleAsync();
        Assert.Equal(user.Id, savedEvent.UserId);
        Assert.Equal(SaveEventResult.Saved, await service.SaveAsync(
            TestIdentityUser.CreatePrincipal(otherUser),
            @event.Id));
        Assert.Equal(2, await dbContext.SavedEvents.CountAsync());
    }

    [Fact]
    public async Task RemoveAsync_changes_only_the_current_users_saved_relationship()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var dbContext = database.CreateDbContext();
        var firstUser = await TestIdentityUser.CreateAsync(dbContext);
        var secondUser = await TestIdentityUser.CreateAsync(dbContext);
        var @event = new Event
        {
            Title = "Upcoming",
            StartAtUtc = DateTime.UtcNow.AddDays(1),
            SourceName = "Test"
        };
        dbContext.Events.Add(@event);
        await dbContext.SaveChangesAsync();
        var firstUsersSavedEvent = new SavedEvent
        {
            UserId = firstUser.Id,
            EventId = @event.Id,
            SavedAtUtc = DateTime.UtcNow
        };
        var secondUsersSavedEvent = new SavedEvent
        {
            UserId = secondUser.Id,
            EventId = @event.Id,
            SavedAtUtc = DateTime.UtcNow
        };
        dbContext.SavedEvents.AddRange(firstUsersSavedEvent, secondUsersSavedEvent);
        await dbContext.SaveChangesAsync();
        var service = new SavedEventsService(dbContext, TimeProvider.System);

        Assert.Equal(
            RemoveSavedEventResult.NotFound,
            await service.RemoveAsync(
                TestIdentityUser.CreatePrincipal(firstUser),
                secondUsersSavedEvent.Id));
        Assert.Equal(
            RemoveSavedEventResult.Removed,
            await service.RemoveAsync(
                TestIdentityUser.CreatePrincipal(firstUser),
                firstUsersSavedEvent.Id));
        Assert.Equal(
            RemoveSavedEventResult.NotFound,
            await service.RemoveAsync(
                TestIdentityUser.CreatePrincipal(firstUser),
                int.MaxValue));

        var remaining = await dbContext.SavedEvents.SingleAsync();
        Assert.Equal(secondUser.Id, remaining.UserId);
        Assert.True(await dbContext.Events.AnyAsync(candidate => candidate.Id == @event.Id));
    }
}
