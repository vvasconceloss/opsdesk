using Microsoft.EntityFrameworkCore;
using OpsDesk.Domain.Entities;

namespace OpsDesk.Infrastructure.Persistence;

public class OpsDeskDbContext(DbContextOptions<OpsDeskDbContext> options) : DbContext(options)
{
  public DbSet<User> Users { get; set; }
  public DbSet<Ticket> Tickets { get; set; }
  public DbSet<TicketCategory> Categories { get; set; }
  public DbSet<TicketComment> Comments { get; set; }
  public DbSet<SlaPolicy> Policies { get; set; }
  public DbSet<Notification> Notifications { get; set; }
  public DbSet<AuditEntry> AuditEntries { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpsDeskDbContext).Assembly);
  }
}