using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HomeownersAssociation.Models;

namespace HomeownersAssociation.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Bill> Bills { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<FacilityReservation> FacilityReservations { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<ForumCategory> ForumCategories { get; set; }
    public DbSet<ForumThread> ForumThreads { get; set; }
    public DbSet<ForumReply> ForumReplies { get; set; }
    
    // New security-related DbSets
    public DbSet<VisitorPass> VisitorPasses { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    
    // Event Calendar
    public DbSet<Event> Events { get; set; }
    
    // Feedback and Complaints
    public DbSet<Feedback> Feedbacks { get; set; }

    // Contact Directory
    public DbSet<Contact> Contacts { get; set; }

    // Polls and Surveys
    public DbSet<Poll> Polls { get; set; }
    public DbSet<PollOption> PollOptions { get; set; }
    public DbSet<PollVote> PollVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Announcement entity
        builder.Entity<Announcement>()
            .HasOne(a => a.Author)
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Bill entity
        builder.Entity<Bill>()
            .HasOne(b => b.Homeowner)
            .WithMany()
            .HasForeignKey(b => b.HomeownerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Payment entity
        builder.Entity<Payment>()
            .HasOne(p => p.Bill)
            .WithMany()
            .HasForeignKey(p => p.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Payment>()
            .HasOne(p => p.Homeowner)
            .WithMany()
            .HasForeignKey(p => p.HomeownerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Payment>()
            .HasOne(p => p.ProcessedBy)
            .WithMany()
            .HasForeignKey(p => p.ProcessedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure FacilityReservation entity
        builder.Entity<FacilityReservation>()
            .HasOne(fr => fr.Facility)
            .WithMany(f => f.Reservations)
            .HasForeignKey(fr => fr.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FacilityReservation>()
            .HasOne(fr => fr.User)
            .WithMany()
            .HasForeignKey(fr => fr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FacilityReservation>()
            .HasIndex(fr => new { fr.FacilityId, fr.StartTime, fr.EndTime });

        // Configure ServiceRequest entity
        builder.Entity<ServiceRequest>()
            .HasOne(sr => sr.User)
            .WithMany()
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Category)
            .WithMany(sc => sc.ServiceRequests)
            .HasForeignKey(sr => sr.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Document entity
        builder.Entity<Document>()
            .HasOne(d => d.UploadedBy)
            .WithMany()
            .HasForeignKey(d => d.UploadedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure ForumThread entity
        builder.Entity<ForumThread>()
            .HasOne(ft => ft.Category)
            .WithMany(fc => fc.ForumThreads)
            .HasForeignKey(ft => ft.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ForumThread>()
            .HasOne(ft => ft.User)
            .WithMany()
            .HasForeignKey(ft => ft.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure ForumReply entity
        builder.Entity<ForumReply>()
            .HasOne(fr => fr.Thread)
            .WithMany(ft => ft.ForumReplies)
            .HasForeignKey(fr => fr.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ForumReply>()
            .HasOne(fr => fr.User)
            .WithMany()
            .HasForeignKey(fr => fr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Configure VisitorPass entity
        builder.Entity<VisitorPass>()
            .HasOne(vp => vp.RequestedBy)
            .WithMany()
            .HasForeignKey(vp => vp.RequestedById)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure Vehicle entity
        builder.Entity<Vehicle>()
            .HasOne(v => v.Owner)
            .WithMany()
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure EmergencyContact entity
        builder.Entity<EmergencyContact>()
            .HasOne(ec => ec.CreatedBy)
            .WithMany()
            .HasForeignKey(ec => ec.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Configure Event entity
        builder.Entity<Event>()
            .HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);
            
        // Configure Feedback entity
        builder.Entity<Feedback>()
            .HasOne(f => f.SubmittedBy)
            .WithMany()
            .HasForeignKey(f => f.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<Feedback>()
            .HasOne(f => f.RespondedBy)
            .WithMany()
            .HasForeignKey(f => f.RespondedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Poll entity
        builder.Entity<Poll>()
            .HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure PollOption entity
        builder.Entity<PollOption>()
            .HasOne(po => po.Poll)
            .WithMany(p => p.Options)
            .HasForeignKey(po => po.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PollVote entity
        builder.Entity<PollVote>()
            .HasOne(pv => pv.Poll)
            .WithMany(p => p.Votes)
            .HasForeignKey(pv => pv.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PollVote>()
            .HasOne(pv => pv.PollOption)
            .WithMany()
            .HasForeignKey(pv => pv.PollOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PollVote>()
            .HasOne(pv => pv.User)
            .WithMany()
            .HasForeignKey(pv => pv.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensure a user can vote only once per poll
        builder.Entity<PollVote>()
            .HasIndex(pv => new { pv.PollId, pv.UserId }).IsUnique();
    }
}
