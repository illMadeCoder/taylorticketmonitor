
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<YourModel> events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("events"); // Add this line to specify the schema
        modelBuilder.Entity<YourModel>(entity =>
        {
            entity.ToTable("events", "events");
        });
    }
}