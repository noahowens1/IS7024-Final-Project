using System.Net;
using CincyEvents.Models;
using CincyEvents.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CincyEvents.Tests.Pages;

public sealed class SaveEventPageTests
{
    [Fact]
    public async Task Signed_in_user_can_save_an_upcoming_event()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var email = $"save-{Guid.NewGuid():N}@example.com";
        using var registration = await IdentityPageTestClient.RegisterAsync(client, email);
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        var eventId = await AddUpcomingEventAsync(factory, "Saved from catalog");
        using var response = await IdentityPageTestClient.PostFormAsync(
            client,
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FEvents",
            "/Events",
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var eventsPage = await client.GetAsync("/Events");
        Assert.Equal(HttpStatusCode.OK, eventsPage.StatusCode);
        Assert.Contains("Event Saved.", await eventsPage.Content.ReadAsStringAsync());

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(1, await dbContext.SavedEvents.CountAsync());
        var user = await dbContext.Users.SingleAsync(candidate => candidate.Email == email);
        Assert.Equal(user.Id, (await dbContext.SavedEvents.SingleAsync()).UserId);
    }

    private static async Task<int> AddUpcomingEventAsync(
        CincyEventsWebApplicationFactory factory,
        string title)
    {
        await using var dbContext = factory.CreateDbContext();
        var @event = new Event
        {
            Title = title,
            StartAtUtc = DateTime.UtcNow.AddDays(1),
            EndAtUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
            SourceName = "Test fixture"
        };
        dbContext.Events.Add(@event);
        await dbContext.SaveChangesAsync();
        return @event.Id;
    }
}
