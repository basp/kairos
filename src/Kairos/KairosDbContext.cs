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
}