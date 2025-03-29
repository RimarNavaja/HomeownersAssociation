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
            .OnDelete(DeleteBehavior.Cascade);

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
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Payment>()
            .HasOne(p => p.ProcessedBy)
            .WithMany()
            .HasForeignKey(p => p.ProcessedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
