using CincyEvents.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CincyEvents.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
    public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EventParticipant>()
            .HasKey(participant => new { participant.EventId, participant.UserId });

        builder.Entity<EventParticipant>()
            .HasOne(participant => participant.Event)
            .WithMany(eventItem => eventItem.Participants)
            .HasForeignKey(participant => participant.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EventParticipant>()
            .HasOne(participant => participant.User)
            .WithMany(user => user.EventParticipations)
            .HasForeignKey(participant => participant.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ChatConversation>()
            .HasOne(conversation => conversation.FirstUser)
            .WithMany()
            .HasForeignKey(conversation => conversation.FirstUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ChatConversation>()
            .HasOne(conversation => conversation.SecondUser)
            .WithMany()
            .HasForeignKey(conversation => conversation.SecondUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ChatMessage>()
            .Property(message => message.Content)
            .HasMaxLength(2000)
            .IsRequired();
    }
}
