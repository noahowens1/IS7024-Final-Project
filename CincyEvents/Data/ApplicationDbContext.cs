using CincyEvents.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CincyEvents.Data;

public sealed class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();

    public DbSet<SavedEvent> SavedEvents => Set<SavedEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Event>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SourceName).IsRequired().HasMaxLength(100);
            entity.ToTable("Events", table =>
            {
                table.HasCheckConstraint(
                    "CK_Events_EndAtUtc_AfterStartAtUtc",
                    "\"EndAtUtc\" IS NULL OR \"EndAtUtc\" >= \"StartAtUtc\"");
                table.HasCheckConstraint(
                    "CK_Events_Prices_NonNegative",
                    "(\"PriceMin\" IS NULL OR \"PriceMin\" >= 0) AND (\"PriceMax\" IS NULL OR \"PriceMax\" >= 0)");
                table.HasCheckConstraint(
                    "CK_Events_PriceMax_AfterPriceMin",
                    "\"PriceMin\" IS NULL OR \"PriceMax\" IS NULL OR \"PriceMax\" >= \"PriceMin\"");
            });
        });

        builder.Entity<SavedEvent>(entity =>
        {
            entity.HasIndex(savedEvent => new { savedEvent.UserId, savedEvent.EventId })
                .IsUnique();

            entity.HasOne(savedEvent => savedEvent.User)
                .WithMany()
                .HasForeignKey(savedEvent => savedEvent.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(savedEvent => savedEvent.Event)
                .WithMany(@event => @event.SavedEvents)
                .HasForeignKey(savedEvent => savedEvent.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
