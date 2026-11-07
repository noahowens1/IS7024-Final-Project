using CincyEvents.Data;
using CincyEvents.Tests.Infrastructure;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CincyEvents.Tests.Data;

public sealed class DevelopmentEventSeederTests
{
    [Fact]
    public async Task SeedAsync_does_not_add_sample_events_outside_development()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var dbContext = database.CreateDbContext();

        await DevelopmentEventSeeder.SeedAsync(dbContext, new TestEnvironment(Environments.Production));

        Assert.Empty(dbContext.Events);
    }

    [Fact]
    public async Task SeedAsync_adds_labeled_development_events_once()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var dbContext = database.CreateDbContext();
        var development = new TestEnvironment(Environments.Development);

        await DevelopmentEventSeeder.SeedAsync(dbContext, development);
        await DevelopmentEventSeeder.SeedAsync(dbContext, development);

        Assert.Equal(2, dbContext.Events.Count());
        Assert.All(dbContext.Events, @event =>
        {
            Assert.True(@event.IsSample);
            Assert.Equal("Development sample catalog", @event.SourceName);
        });
    }

    private sealed class TestEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "CincyEvents.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
