using System.Security.Claims;
using CincyEvents.Models;
using CincyEvents.Services;
using CincyEvents.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CincyEvents.Tests.Services;

public sealed class SavedEventsServiceTests
{
    [Theory]
    [InlineData("https://example.org/tickets", true)]
    [InlineData("http://example.org/tickets", true)]
    [InlineData("ftp://example.org/tickets", false)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("/relative/path", false)]
    public void Event_ticket_url_accepts_only_absolute_http_or_https_urls(string ticketUrl, bool isValid)
    {
        var validationContext = new ValidationContext(new Event { TicketUrl = ticketUrl })
        {
            MemberName = nameof(Event.TicketUrl)
        };
        var validationResults = new List<ValidationResult>();

        var actual = Validator.TryValidateProperty(
            ticketUrl,
            validationContext,
            validationResults);

        Assert.Equal(isValid, actual);
    }

    [Fact]
    public async Task SaveAsync_saves_only_valid_upcoming_events()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var dbContext = database.CreateDbContext();
        var user = await TestIdentityUser.CreateAsync(dbContext);
        var now = DateTime.UtcNow;
        var upcoming = new Event
        {
            Title = "Upcoming",
            StartAtUtc = now.AddDays(1),
            EndAtUtc = now.AddDays(1).AddHours(1),
            SourceName = "Test"
        };
        var started = new Event
        {
            Title = "Started",
            StartAtUtc = now.AddHours(-1),
            EndAtUtc = now.AddHours(1),
            SourceName = "Test"
        };
        var ended = new Event
        {
            Title = "Ended",
            StartAtUtc = now.AddDays(-2),
            EndAtUtc = now.AddDays(-1),
            SourceName = "Test"
        };
        dbContext.Events.AddRange(upcoming, started, ended);
        await dbContext.SaveChangesAsync();

        var service = new SavedEventsService(dbContext, TimeProvider.System);
        var principal = TestIdentityUser.CreatePrincipal(user);

        Assert.Equal(SaveEventResult.Saved, await service.SaveAsync(principal, upcoming.Id));
        Assert.Equal(SaveEventResult.EventUnavailable, await service.SaveAsync(principal, started.Id));
        Assert.Equal(SaveEventResult.EventUnavailable, await service.SaveAsync(principal, ended.Id));
        Assert.Equal(SaveEventResult.EventNotFound, await service.SaveAsync(principal, int.MaxValue));
        Assert.Equal(SaveEventResult.InvalidEventId, await service.SaveAsync(principal, 0));
        Assert.Equal(1, dbContext.SavedEvents.Count());
        Assert.Equal(user.Id, dbContext.SavedEvents.Single().UserId);
    }
}

internal static class TestIdentityUser
{
    public static async Task<IdentityUser> CreateAsync(
        CincyEvents.Data.ApplicationDbContext dbContext)
    {
        var user = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"test-{Guid.NewGuid():N}@example.com",
            Email = $"test-{Guid.NewGuid():N}@example.com"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public static ClaimsPrincipal CreatePrincipal(IdentityUser user)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, user.Id)],
            "SavedEventsServiceTest");
        return new ClaimsPrincipal(identity);
    }
}
