namespace Kairos;

using Entities;
using Microsoft.EntityFrameworkCore;

public class KairosDbContext(DbContextOptions<KairosDbContext> options)
    : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }

    public DbSet<Workspace> Workspaces { get; set; }
    
    public DbSet<Timer> Timers { get; set; }
    
    public DbSet<Tag> Tags { get; set; }
    
    public DbSet<TimerTag> TimerTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasMany(a => a.Workspaces)
                .WithOne(w => w.Account)
                .HasForeignKey(w => w.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasMany(w => w.Timers)
                .WithOne(t => t.Workspace)
                .HasForeignKey(t => t.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(w => w.Tags)
                .WithOne(t => t.Workspace)
                .HasForeignKey(t => t.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Timer>(entity =>
        {
            entity.HasMany(t => t.Tags)
                .WithMany(t => t.Timers)
                .UsingEntity<TimerTag>(
                    r => r.HasOne(tt => tt.Tag).WithMany().HasForeignKey(tt => tt.TagId).OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne(tt => tt.Timer).WithMany().HasForeignKey(tt => tt.TimerId).OnDelete(DeleteBehavior.ClientCascade),
                    j =>
                    {
                        j.HasKey(tt => new { tt.TimerId, tt.TagId });
                    });
        });
    }
}