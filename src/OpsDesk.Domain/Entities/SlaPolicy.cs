using OpsDesk.Domain.Enums;

namespace OpsDesk.Domain.Entities;

public class SlaPolicy
{
  public Guid Id { get; init; }
  public Priority Priority { get; set; }
  public int ResponseHours { get; set; }
}