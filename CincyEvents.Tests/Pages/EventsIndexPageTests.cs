using System.Net;
using CincyEvents.Models;
using CincyEvents.Services;
using CincyEvents.Tests.Infrastructure;
using Xunit;

namespace CincyEvents.Tests.Pages;

public sealed class EventsIndexPageTests
{
    [Fact]
    public async Task Upcoming_catalog_labels_sample_data_and_displays_local_event_time()
    {
        using var factory = new CincyEventsWebApplicationFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var startAtUtc = DateTime.UtcNow.AddDays(7);
        await using (var dbContext = factory.CreateDbContext())
        {
            dbContext.Events.Add(new Event
            {
                Title = "Cincinnati local time test",
                StartAtUtc = startAtUtc,
                TimeZoneId = "Eastern Standard Time",
                SourceName = "Development sample catalog",
                IsSample = true
            });
            await dbContext.SaveChangesAsync();
        }

        using var response = await client.GetAsync("/Events");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Sample event - not live event information", html);
        Assert.Contains(EventTimeFormatter.Format(startAtUtc, "Eastern Standard Time"), html);
    }
}
