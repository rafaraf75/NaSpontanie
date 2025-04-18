using Microsoft.EntityFrameworkCore;
using NaSpontanie.API.Models;

namespace NaSpontanie.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<EventInterest> EventInterests { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<JoinRequest> JoinRequests { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Wydarzenie <-> Zainteresowanie (many-to-many)
            modelBuilder.Entity<EventInterest>()
                .HasKey(ei => new { ei.EventId, ei.InterestId });

            modelBuilder.Entity<EventInterest>()
                .HasOne(ei => ei.Event)
                .WithMany(e => e.EventInterests)
                .HasForeignKey(ei => ei.EventId);

            modelBuilder.Entity<EventInterest>()
                .HasOne(ei => ei.Interest)
                .WithMany(i => i.EventInterests)
                .HasForeignKey(ei => ei.InterestId);

            // Znajomi (many-to-many na Friendship)
            modelBuilder.Entity<Friendship>()
                .HasKey(f => new { f.UserId, f.FriendId });

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // Komentarz -> Użytkownik
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Komentarz -> Event
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Comments)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Zgłoszenie -> Zgłaszający
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Zgłoszenie -> Użytkownik zgłoszony
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Zgłoszenie -> Event zgłoszony
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReportedEvent)
                .WithMany()
                .HasForeignKey(r => r.ReportedEventId)
                .OnDelete(DeleteBehavior.Restrict);

            // JoinRequest -> User
            modelBuilder.Entity<JoinRequest>()
                .HasOne(jr => jr.User)
                .WithMany()
                .HasForeignKey(jr => jr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // JoinRequest -> Event
            modelBuilder.Entity<JoinRequest>()
                .HasOne(jr => jr.Event)
                .WithMany()
                .HasForeignKey(jr => jr.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
