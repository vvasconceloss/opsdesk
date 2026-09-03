namespace OpsDesk.Domain.Enums;

public enum AuditAction
{
  TicketCreated,
  TicketAssigned,
  PriorityChanged,
  StatusChanged,
  CommentAdded,
  TicketResolved,
  TicketReopened,
  TicketClosed
}