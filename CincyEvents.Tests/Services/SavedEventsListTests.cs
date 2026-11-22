using CincyEvents.Models;
using CincyEvents.Services;
using CincyEvents.Tests.Infrastructure;
using Xunit;

namespace CincyEvents.Tests.Services;

public sealed class SavedEventsListTests
{
    [Fact]
    public async Task ListAsync_returns_only_the_current_users_event_details()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var dbContext = database.CreateDbContext();
        var firstUser = await TestIdentityUser.CreateAsync(dbContext);
        var secondUser = await TestIdentityUser.CreateAsync(dbContext);
        var @event = new Event
        {
            Title = "Price-free sample",
            Category = "Arts",
            StartAtUtc = DateTime.UtcNow.AddDays(1),
            TimeZoneId = "Eastern Standard Time",
            Venue = "Test venue",
            SourceName = "Test",
            IsSample = true
        };
        dbContext.Events.Add(@event);
        await dbContext.SaveChangesAsync();
        dbContext.SavedEvents.Add(new SavedEvent
        {
            UserId = firstUser.Id,
            EventId = @event.Id,
            SavedAtUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = new SavedEventsService(dbContext, TimeProvider.System);
        var firstUsersEvents = await service.ListAsync(TestIdentityUser.CreatePrincipal(firstUser));
        var secondUsersEvents = await service.ListAsync(TestIdentityUser.CreatePrincipal(secondUser));

        var savedEvent = Assert.Single(firstUsersEvents);
        Assert.Equal(@event.Title, savedEvent.Title);
        Assert.Equal(@event.Category, savedEvent.Category);
        Assert.Equal(@event.Venue, savedEvent.Venue);
        Assert.Null(savedEvent.PriceMin);
        Assert.True(savedEvent.IsSample);
        Assert.Empty(secondUsersEvents);
    }
}
