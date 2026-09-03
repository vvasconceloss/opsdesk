using OpsDesk.Domain.Enums;

namespace OpsDesk.Domain.Entities;

public class User
{
  public Guid Id { get; init; }
  public required string Email { get; set; }
  public string? DisplayName { get; set; }
  public Role Role { get; set; }
  public bool IsActive { get; set; } = true;
}