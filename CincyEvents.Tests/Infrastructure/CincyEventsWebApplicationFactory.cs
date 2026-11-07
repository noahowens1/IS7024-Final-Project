using CincyEvents.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CincyEvents.Tests.Infrastructure;

public sealed class CincyEventsWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"cincy-events-web-tests-{Guid.NewGuid():N}.db");
    private string ConnectionString => new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
    {
        DataSource = _databasePath,
        Pooling = false
    }.ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.Sources.Clear();
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApplicationDbContext>();
            foreach (var descriptor in services.Where(descriptor =>
                         descriptor.ServiceType.IsGenericType
                         && descriptor.ServiceType.GenericTypeArguments.Contains(typeof(ApplicationDbContext))
                         && descriptor.ServiceType.FullName?.Contains(
                             "DbContextOptions",
                             StringComparison.Ordinal) == true).ToArray())
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(ConnectionString));
        });
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
