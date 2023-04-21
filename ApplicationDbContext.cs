
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> events { get; set; }
    public DbSet<RmEvent> rmEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("events"); // Add this line to specify the schema
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events", "events");
        });
        modelBuilder.Entity<RmEvent>(entity =>
        {
            entity.ToTable("rm_events", "events");
        });
    }
}