using CincyEvents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CincyEvents.Data;

public static class DevelopmentEventSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        IHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment()
            || await dbContext.Events.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        dbContext.Events.AddRange(
            new Event
            {
                Id = 1,
                Title = "Sample Cincinnati Music Night",
                Category = "Music",
                StartAtUtc = now.AddDays(7),
                EndAtUtc = now.AddDays(7).AddHours(2),
                TimeZoneId = "Eastern Standard Time",
                Venue = "Sample venue",
                PriceMin = 15,
                PriceMax = 25,
                SourceName = "Development sample catalog",
                IsSample = true
            },
            new Event
            {
                Id = 2,
                Title = "Sample Cincinnati Art Walk",
                Category = "Arts",
                StartAtUtc = now.AddDays(14),
                EndAtUtc = now.AddDays(14).AddHours(3),
                TimeZoneId = "Eastern Standard Time",
                Venue = "Sample downtown location",
                SourceName = "Development sample catalog",
                IsSample = true
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
