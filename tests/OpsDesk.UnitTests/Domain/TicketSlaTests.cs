using OpsDesk.Domain.Entities;
using OpsDesk.Domain.Enums;

namespace OpsDesk.UnitTests.Domain;

public class TicketSlaTests
{
  private readonly DateTimeOffset _baseTime = new(2026, 9, 3, 14, 0, 0, TimeSpan.Zero); 

  private Ticket CreateTicketWithDeadline(DateTimeOffset deadline) => new()
  {
    ReferenceNumber = "OPS-000002",
    Title = "Critical Issue",
    CreatedAt = _baseTime,
    SlaDeadline = deadline
  };

  [Fact]
  public void CalculateSlaState_WhenTimeIsWayBeforeDeadline_ShouldBeOnTrack()
  {
    DateTimeOffset deadline = _baseTime.AddHours(4); 
    Ticket ticket = CreateTicketWithDeadline(deadline);

    SlaState state = ticket.CalculateSlaState(_baseTime.AddHours(1));
    
    Assert.Equal(SlaState.OnTrack, state);
  }
  
  [Fact]
  public void CalculateSlaState_WhenDeadlineIsApproaching_ShouldBeAtRisk()
  {
    DateTimeOffset deadline = _baseTime.AddHours(4); 
    Ticket ticket = CreateTicketWithDeadline(deadline);
    
    SlaState state = ticket.CalculateSlaState(_baseTime.AddHours(3).AddMinutes(45));
    
    Assert.Equal(SlaState.AtRisk, state);
  }

  [Fact]
  public void CalculateSlaState_WhenTimeHasPassedDeadline_ShouldBeBreached()
  {
    DateTimeOffset deadline = _baseTime.AddHours(4); 
    Ticket ticket = CreateTicketWithDeadline(deadline);
      
    SlaState state = ticket.CalculateSlaState(_baseTime.AddHours(4).AddMinutes(1));
      
    Assert.Equal(SlaState.Breached, state);
  }
}