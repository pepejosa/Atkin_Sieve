using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RequestHistory> RequestHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(u => u.Username)
                .IsUnique();
            entity.HasIndex(u => u.Email)
                .IsUnique();
        });
        
        modelBuilder.Entity<RequestHistory>(entity =>
        {
            entity.ToTable("RequestHistory");
            entity.HasOne<User>()
                .WithMany(u => u.RequestHistory)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 