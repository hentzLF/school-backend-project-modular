using AgriMarket.Modules.Messaging.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Messaging.Persistence;

public sealed class MessagingDbContext : DbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; set; } = default!;
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = default!;
    public DbSet<Message> Messages { get; set; } = default!;
    public DbSet<MessageRead> MessageReads { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("messaging");

        // ConversationParticipant: composite PK (ConversationId, UserProfileId)
        modelBuilder.Entity<ConversationParticipant>()
            .HasKey(cp => new { cp.ConversationId, cp.UserProfileId });

        // MessageRead: a user can only read a message once
        modelBuilder.Entity<MessageRead>()
            .HasIndex(mr => new { mr.MessageId, mr.UserProfileId })
            .IsUnique();

        // Conversation → Message: cascade delete messages with conversation
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Message → MessageRead: cascade delete reads with message
        modelBuilder.Entity<MessageRead>()
            .HasOne(mr => mr.Message)
            .WithMany(m => m.MessageReads)
            .HasForeignKey(mr => mr.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on Message.SentAt for query performance
        modelBuilder.Entity<Message>()
            .HasIndex(m => m.SentAt);
    }
}
