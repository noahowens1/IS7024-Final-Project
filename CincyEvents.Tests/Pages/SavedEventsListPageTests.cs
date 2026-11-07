using System.Net;
using CincyEvents.Models;
using CincyEvents.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CincyEvents.Tests.Pages;

public sealed class SavedEventsListPageTests
{
    [Fact]
    public async Task User_sees_saved_details_and_unavailable_price()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var email = $"list-{Guid.NewGuid():N}@example.com";
        using var registration = await IdentityPageTestClient.RegisterAsync(client, email);
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        int eventId;
        await using (var dbContext = factory.CreateDbContext())
        {
            var @event = new Event
            {
                Title = "Personal saved listing",
                Category = "Community",
                StartAtUtc = DateTime.UtcNow.AddDays(1),
                TimeZoneId = "Eastern Standard Time",
                Venue = "Test venue",
                SourceName = "Test fixture",
                IsSample = true
            };
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            eventId = @event.Id;
        }

        using var save = await IdentityPageTestClient.PostFormAsync(
            client,
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FSavedEvents",
            "/Events",
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, save.StatusCode);

        using var savedEvents = await client.GetAsync("/SavedEvents");
        var html = await savedEvents.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, savedEvents.StatusCode);
        Assert.Contains("Personal saved listing", html);
        Assert.Contains("Test venue", html);
        Assert.Contains("Price unavailable", html);
        Assert.Contains("Sample event - not live event information", html);
    }

    [Fact]
    public async Task New_user_sees_an_informative_empty_state()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var registration = await IdentityPageTestClient.RegisterAsync(
            client,
            $"empty-{Guid.NewGuid():N}@example.com");

        using var response = await client.GetAsync("/SavedEvents");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("haven't saved any events yet", await response.Content.ReadAsStringAsync());
    }
}
