using OpsDesk.Domain.Enums;

namespace OpsDesk.Domain.Entities;

public class Ticket
{
  public Guid Id { get; init; }
  public required string ReferenceNumber { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public TicketStatus Status { get; set; }
  public Priority Priority { get; set; }
  public Guid CategoryId { get; set; }
  public Guid CreatedByUserId { get; set; }
  public Guid? AssignedToUserId { get; set; }
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public DateTimeOffset? UpdatedAt { get; set; }
  public DateTimeOffset? SlaDeadline { get; set; }
  public DateTimeOffset? ResolvedAt { get; set; }
  public DateTimeOffset? ClosedAt { get; set; }

  public void ChangeStatus(TicketStatus newStatus, DateTimeOffset now)
  {
    if (Status == newStatus) return;

    bool isTransitionValid = (Status, newStatus) switch
    {
      (TicketStatus.New, TicketStatus.Assigned)        => true,
      (TicketStatus.New, TicketStatus.InProgress)      => true,

      (TicketStatus.Assigned, TicketStatus.InProgress) => true,
      (TicketStatus.InProgress, TicketStatus.Resolved) => true,
      
      (TicketStatus.Resolved, TicketStatus.Closed)     => true,
      (TicketStatus.Resolved, TicketStatus.Reopened)   => true,

      (TicketStatus.Reopened, TicketStatus.InProgress) => true,

      (TicketStatus.Closed, _)                         => false,
      _ => false 
    };

    if (!isTransitionValid)
      throw new InvalidOperationException($"Invalid state transition: It is not permitted to change from {Status} to {newStatus}.");
    
    switch (newStatus)
    {
      case TicketStatus.Resolved:
        ResolvedAt = now;
        break;
      case TicketStatus.Closed:
        ClosedAt = now;
        break;
      case TicketStatus.Reopened:
        ResolvedAt = null;
        ClosedAt = null;
        break;
    }

    Status = newStatus;
    UpdatedAt = now;
  }

  public SlaState CalculateSlaState(DateTimeOffset now)
  {
    if (ResolvedAt.HasValue && SlaDeadline.HasValue)
      return ResolvedAt.Value <= SlaDeadline.Value ? SlaState.OnTrack : SlaState.Breached;

    if (!SlaDeadline.HasValue) return SlaState.OnTrack;

    if (now > SlaDeadline.Value) return SlaState.Breached;

    TimeSpan remainingTime = SlaDeadline.Value - now;

    if (remainingTime <= TimeSpan.FromMinutes(30)) return SlaState.AtRisk;

    return SlaState.OnTrack;
  }
}