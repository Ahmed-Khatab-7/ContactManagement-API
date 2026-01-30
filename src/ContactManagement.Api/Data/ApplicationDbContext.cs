using ContactManagement.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContactManagement.Api.Data;

/// <summary>
/// Database context that combines Identity tables with our custom entities.
/// IdentityDbContext provides Users, Roles, Claims tables automatically.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<Contact> Contacts => Set<Contact>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure Contact entity
        builder.Entity<Contact>(entity =>
        {
            // Index on UserId for faster queries (filtering by user)
            entity.HasIndex(c => c.UserId);
            
            // Composite index for common query patterns
            entity.HasIndex(c => new { c.UserId, c.LastName, c.FirstName });
            
            // Unique email per user (a user can't have duplicate contact emails)
            entity.HasIndex(c => new { c.UserId, c.Email }).IsUnique();
            
            // Configure relationship
            entity.HasOne(c => c.User)
                  .WithMany(u => u.Contacts)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // Delete contacts when user is deleted
        });
        
        // Customize Identity table names (optional, cleaner naming)
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
        });
    }
}
