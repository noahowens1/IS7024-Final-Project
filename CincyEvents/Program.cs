using CincyEvents.Data;
using CincyEvents.Hubs;
using CincyEvents.Models;
using CincyEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.AddSignalR();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<EventDiscoveryService>();
builder.Services.AddMemoryCache();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('dbo.Events', 'ExternalId') IS NULL
                ALTER TABLE [dbo].[Events] ADD [ExternalId] nvarchar(450) NULL;
            """);

        db.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('dbo.Events', 'Source') IS NULL
                ALTER TABLE [dbo].[Events] ADD [Source] nvarchar(450) NULL;
            """);

        db.Database.ExecuteSqlRaw("""
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_Events_Source_ExternalId'
                    AND object_id = OBJECT_ID(N'[dbo].[Events]'))
                CREATE UNIQUE INDEX [IX_Events_Source_ExternalId]
                    ON [dbo].[Events] ([Source], [ExternalId])
                    WHERE [Source] IS NOT NULL AND [ExternalId] IS NOT NULL;
            """);
    }

    if (db.ChangeTracker.HasChanges())
        db.SaveChanges();
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

app.MapHub<ChatHub>("/hubs/chat");
app.MapRazorPages();

app.Run(); // Binding to http://localhost:5144
