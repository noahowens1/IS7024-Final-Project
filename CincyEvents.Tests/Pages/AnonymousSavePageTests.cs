using System.Net;
using CincyEvents.Models;
using CincyEvents.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CincyEvents.Tests.Pages;

public sealed class AnonymousSavePageTests
{
    [Fact]
    public async Task Anonymous_save_redirects_to_login_without_replaying_after_registration()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var eventId = await AddUpcomingEventAsync(factory);

        using var anonymousSave = await IdentityPageTestClient.PostFormAsync(
            client,
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FEvents",
            "/Events",
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, anonymousSave.StatusCode);
        Assert.Contains("/Identity/Account/Login", anonymousSave.Headers.Location?.ToString());

        await using (var dbContext = factory.CreateDbContext())
        {
            Assert.Empty(dbContext.SavedEvents);
        }

        using var registration = await IdentityPageTestClient.RegisterAsync(
            client,
            $"anonymous-retry-{Guid.NewGuid():N}@example.com");
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);
        await using (var dbContext = factory.CreateDbContext())
        {
            Assert.Empty(dbContext.SavedEvents);
        }

        using var retry = await IdentityPageTestClient.PostFormAsync(
            client,
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FEvents",
            "/Events",
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, retry.StatusCode);
        await using var finalDbContext = factory.CreateDbContext();
        Assert.Equal(1, await finalDbContext.SavedEvents.CountAsync());
    }

    [Fact]
    public async Task External_return_url_is_rejected_after_save()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var registration = await IdentityPageTestClient.RegisterAsync(
            client,
            $"safe-redirect-{Guid.NewGuid():N}@example.com");
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        var eventId = await AddUpcomingEventAsync(factory);
        using var response = await IdentityPageTestClient.PostFormAsync(
            client,
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=https%3A%2F%2Fexample.org",
            "/Events",
            new Dictionary<string, string>());

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Events", response.Headers.Location?.ToString());
    }

    private static async Task<int> AddUpcomingEventAsync(CincyEventsWebApplicationFactory factory)
    {
        await using var dbContext = factory.CreateDbContext();
        var @event = new Event
        {
            Title = "Anonymous-save test event",
            StartAtUtc = DateTime.UtcNow.AddDays(1),
            SourceName = "Test fixture"
        };
        dbContext.Events.Add(@event);
        await dbContext.SaveChangesAsync();
        return @event.Id;
    }
}
