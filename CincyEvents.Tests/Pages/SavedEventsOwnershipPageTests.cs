using System.Net;
using CincyEvents.Models;
using CincyEvents.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CincyEvents.Tests.Pages;

public sealed class SavedEventsOwnershipPageTests
{
    [Fact]
    public async Task Concurrent_identity_saves_create_one_user_event_relationship()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var firstClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var secondClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        var email = $"concurrent-{Guid.NewGuid():N}@example.com";
        using var registration = await IdentityPageTestClient.RegisterAsync(firstClient, email);
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);
        using var login = await IdentityPageTestClient.PostFormAsync(
            secondClient,
            "/Identity/Account/Login",
            "/Identity/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = "Student1!"
            });
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        using var secondAuthCheck = await secondClient.GetAsync("/SavedEvents");
        Assert.Equal(HttpStatusCode.OK, secondAuthCheck.StatusCode);

        int eventId;
        await using (var dbContext = factory.CreateDbContext())
        {
            var @event = new Event
            {
                Title = "Concurrent save event",
                StartAtUtc = DateTime.UtcNow.AddDays(1),
                SourceName = "Test fixture"
            };
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            eventId = @event.Id;
        }

        var firstToken = await IdentityPageTestClient.GetAntiforgeryTokenAsync(firstClient, "/Events");
        var secondToken = await IdentityPageTestClient.GetAntiforgeryTokenAsync(secondClient, "/Events");
        var savePath = $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FEvents";
        var concurrentSaves = await Task.WhenAll(
            firstClient.PostAsync(savePath, FormWithToken(firstToken)),
            secondClient.PostAsync(savePath, FormWithToken(secondToken)));
        using var firstSave = concurrentSaves[0];
        using var secondSave = concurrentSaves[1];

        Assert.All(concurrentSaves, response => Assert.Equal(HttpStatusCode.Redirect, response.StatusCode));
        Assert.DoesNotContain("/Identity/Account/Login", firstSave.Headers.Location?.ToString());
        Assert.DoesNotContain("/Identity/Account/Login", secondSave.Headers.Location?.ToString());
        using var firstStatus = await firstClient.GetAsync("/Events");
        using var secondStatus = await secondClient.GetAsync("/Events");
        var firstHtml = await firstStatus.Content.ReadAsStringAsync();
        var secondHtml = await secondStatus.Content.ReadAsStringAsync();
        Assert.True(
            firstHtml.Contains("Event Saved.", StringComparison.Ordinal)
            || secondHtml.Contains("Event Saved.", StringComparison.Ordinal),
            "Neither concurrent save displayed the success message.");
        Assert.True(
            firstHtml.Contains("already saved", StringComparison.OrdinalIgnoreCase)
            || secondHtml.Contains("already saved", StringComparison.OrdinalIgnoreCase),
            $"A concurrent duplicate outcome was not shown. First: {ExtractStatus(firstHtml)}; "
            + $"second: {ExtractStatus(secondHtml)}.");

        await using var savedDbContext = factory.CreateDbContext();
        Assert.Equal(1, await savedDbContext.SavedEvents.CountAsync());
    }

    [Fact]
    public async Task User_cannot_view_or_remove_another_users_saved_event()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var firstClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var secondClient = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var firstRegistration = await IdentityPageTestClient.RegisterAsync(
            firstClient,
            $"owner-{Guid.NewGuid():N}@example.com");
        Assert.Equal(HttpStatusCode.Redirect, firstRegistration.StatusCode);

        int eventId;
        string savedEventOwnerId;
        int savedEventId;
        await using (var dbContext = factory.CreateDbContext())
        {
            var @event = new Event
            {
                Title = "Private saved event",
                StartAtUtc = DateTime.UtcNow.AddDays(1),
                SourceName = "Test fixture"
            };
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            eventId = @event.Id;
        }

        using (var save = await IdentityPageTestClient.PostFormAsync(
            firstClient,
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FSavedEvents",
            "/Events",
            new Dictionary<string, string>()))
        {
            Assert.Equal(HttpStatusCode.Redirect, save.StatusCode);
        }

        await using (var dbContext = factory.CreateDbContext())
        {
            var firstUser = await dbContext.Users.SingleAsync(user => user.Email!.StartsWith("owner-"));
            var savedEvent = await dbContext.SavedEvents.SingleAsync();
            savedEventOwnerId = firstUser.Id;
            savedEventId = savedEvent.Id;
            Assert.Equal(firstUser.Id, savedEvent.UserId);
        }

        using var secondRegistration = await IdentityPageTestClient.RegisterAsync(
            secondClient,
            $"not-owner-{Guid.NewGuid():N}@example.com");
        Assert.Equal(HttpStatusCode.Redirect, secondRegistration.StatusCode);

        using var secondUsersPage = await secondClient.GetAsync("/SavedEvents");
        Assert.Equal(HttpStatusCode.OK, secondUsersPage.StatusCode);
        Assert.DoesNotContain("Private saved event", await secondUsersPage.Content.ReadAsStringAsync());

        using var remove = await IdentityPageTestClient.PostFormAsync(
            secondClient,
            $"/SavedEvents?handler=Remove&savedEventId={savedEventId}",
            "/SavedEvents",
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, remove.StatusCode);
        using var secondUsersResult = await secondClient.GetAsync("/SavedEvents");
        Assert.Contains("could not be found", await secondUsersResult.Content.ReadAsStringAsync());

        using var firstUsersPage = await firstClient.GetAsync("/SavedEvents");
        Assert.Contains("Private saved event", await firstUsersPage.Content.ReadAsStringAsync());
        await using var finalDbContext = factory.CreateDbContext();
        var remaining = await finalDbContext.SavedEvents.SingleAsync();
        Assert.Equal(savedEventOwnerId, remaining.UserId);
    }

    [Fact]
    public async Task Repeated_post_saves_remain_unique_and_remove_requires_antiforgery()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var registration = await IdentityPageTestClient.RegisterAsync(
            client,
            $"duplicate-{Guid.NewGuid():N}@example.com");
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        int eventId;
        await using (var dbContext = factory.CreateDbContext())
        {
            var @event = new Event
            {
                Title = "Unique saved event",
                StartAtUtc = DateTime.UtcNow.AddDays(1),
                SourceName = "Test fixture"
            };
            dbContext.Events.Add(@event);
            await dbContext.SaveChangesAsync();
            eventId = @event.Id;
        }

        using var missingSaveToken = await client.PostAsync(
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FEvents",
            new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.BadRequest, missingSaveToken.StatusCode);

        using var firstSave = await IdentityPageTestClient.PostFormAsync(
            client,
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FEvents",
            "/Events",
            new Dictionary<string, string>());
        using var duplicateSave = await IdentityPageTestClient.PostFormAsync(
            client,
            $"/SavedEvents/Save?eventId={eventId}&returnUrl=%2FEvents",
            "/Events",
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, firstSave.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, duplicateSave.StatusCode);

        using var resultPage = await client.GetAsync("/Events");
        Assert.Contains("already saved", await resultPage.Content.ReadAsStringAsync());
        await using var savedDbContext = factory.CreateDbContext();
        var savedEvent = await savedDbContext.SavedEvents.SingleAsync();

        using var missingTokenRequest = await client.PostAsync(
            $"/SavedEvents?handler=Remove&savedEventId={savedEvent.Id}",
            new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.BadRequest, missingTokenRequest.StatusCode);
        Assert.Single(await savedDbContext.SavedEvents.ToListAsync());
    }

    private static FormUrlEncodedContent FormWithToken(string token)
    {
        return new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });
    }

    private static string ExtractStatus(string html)
    {
        var marker = "role=\"status\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return "(no status)";
        }

        var contentStart = html.IndexOf('>', start) + 1;
        var contentEnd = html.IndexOf("</div>", contentStart, StringComparison.Ordinal);
        return contentEnd < 0 ? html[contentStart..] : html[contentStart..contentEnd];
    }
}
