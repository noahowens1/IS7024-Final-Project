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

    var sampleEvents = new[]
    {
        new Event
        {
            Name = "Cincy Events Community Meetup",
            Description = "Join this event to try the real-time event chat.",
            StartsAt = DateTime.UtcNow.AddDays(7)
        },
        new Event
        {
            Name = "Cincinnati Food Truck Festival",
            Description = "Connect with other attendees and share your favorite food trucks.",
            StartsAt = DateTime.UtcNow.AddDays(14)
        }
    };

    foreach (var sampleEvent in sampleEvents)
    {
        if (!db.Events.Any(eventItem => eventItem.Name == sampleEvent.Name))
            db.Events.Add(sampleEvent);
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
