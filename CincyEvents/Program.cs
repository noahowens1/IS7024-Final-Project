using CincyEvents.Data;
using CincyEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuredConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("The DefaultConnection connection string is not configured.");
var sqliteConnection = new SqliteConnectionStringBuilder(configuredConnectionString);
if (sqliteConnection.Mode != SqliteOpenMode.Memory
    && !sqliteConnection.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
    && !sqliteConnection.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
    && !Path.IsPathRooted(sqliteConnection.DataSource))
{
    sqliteConnection.DataSource = Path.GetFullPath(
        Path.Combine(builder.Environment.ContentRootPath, sqliteConnection.DataSource));
}

if (sqliteConnection.Mode != SqliteOpenMode.Memory
    && !sqliteConnection.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
    && !sqliteConnection.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
{
    var databaseDirectory = Path.GetDirectoryName(sqliteConnection.DataSource);
    if (!string.IsNullOrEmpty(databaseDirectory))
    {
        Directory.CreateDirectory(databaseDirectory);
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(sqliteConnection.ToString()));
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<SavedEventsService>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    await DevelopmentEventSeeder.SeedAsync(dbContext, app.Environment);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program { }
