namespace OpsDesk.Domain.Entities;

public enum TicketCommentType
{
  Public,
  Internal
}

public class TicketComment
{
  public Guid Id { get; init; }
  public Guid TicketId { get; set; }
  public Guid AuthorId { get; set; }
  public TicketCommentType Type { get; set; }
  public required string Body { get; set; }
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}