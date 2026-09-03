using OpsDesk.Domain.Enums;

namespace OpsDesk.Domain.Entities;

public class Notification
{
  public Guid Id { get; init; }
  public Guid UserId { get; set; }
  public NotificationType Type { get; set; }
  public required string Message { get; set; }
  public Guid TicketId { get; set; }
  public bool IsRead { get; set; } = false;
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}