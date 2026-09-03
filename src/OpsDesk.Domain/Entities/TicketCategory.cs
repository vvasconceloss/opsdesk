namespace OpsDesk.Domain.Entities;

public class TicketCategory
{
  public Guid Id { get; init; }
  public required string Name { get; set; }
  public bool IsActive { get; set; } = true;
}