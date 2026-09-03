using OpsDesk.Domain.Enums;

namespace OpsDesk.Domain.Entities;

public class AuditEntry
{
  public Guid Id { get; init; }
  public Guid TicketId { get; set; }
  public Guid ActorId { get; init; }
  public AuditAction Action { get; init; }
  public string? OldValue { get; init; }
  public string? NewValue { get; init; }
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}