
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> events { get; set; }
    public DbSet<RmEvent> rmEvents { get; set; }
    public DbSet<LocationDays> locationDays { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("events"); 
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events", "events");
        });
        
        modelBuilder.Entity<RmEvent>(entity =>
        {
            entity.ToTable("rm_events", "events");
        });
        modelBuilder.Entity<LocationDays>(entity =>
        {
            entity.ToTable("locationdays", "events");
        });        
    }
}