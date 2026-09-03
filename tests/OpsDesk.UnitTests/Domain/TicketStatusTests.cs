using OpsDesk.Domain.Entities;
using OpsDesk.Domain.Enums;

namespace OpsDesk.UnitTests.Domain;

public class TicketStatusTests
{
  private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

  private static Ticket CreateTestTicket() => new()
  {
    ReferenceNumber = "OPS-000001",
    Title = "WiFi problem",
    Description = "I can't connect to the company network"
  };

  [Fact]
  public void ChangeStatus_WhenTransitionIsValid_ShouldUpdateStatusAndTimestamps()
  {
    Ticket ticket = CreateTestTicket();

    ticket.ChangeStatus(TicketStatus.Assigned, _now);

    Assert.Equal(TicketStatus.Assigned, ticket.Status);
    Assert.Equal(_now, ticket.UpdatedAt);
  }

  [Fact]
  public void ChangeStatus_WhenTicketIsClosed_AnyTransitionShouldThrowException()
  {
    Ticket ticket = CreateTestTicket();
    ticket.ChangeStatus(TicketStatus.Assigned, _now);
    ticket.ChangeStatus(TicketStatus.InProgress, _now);
    ticket.ChangeStatus(TicketStatus.Resolved, _now);
    ticket.ChangeStatus(TicketStatus.Closed, _now);

    Exception exception = Assert.Throws<InvalidOperationException>(() =>
      ticket.ChangeStatus(TicketStatus.InProgress, _now)
    );

    Assert.Contains("Invalid state transition", exception.Message);
  }

  [Theory]
  [InlineData(TicketStatus.New, TicketStatus.Closed)]
  [InlineData(TicketStatus.Assigned, TicketStatus.Closed)]
  [InlineData(TicketStatus.InProgress, TicketStatus.Closed)]
  public void ChangeStatus_WhenTransitionIsForbidden_ShouldThrowException(TicketStatus current, TicketStatus invalidNext)
  {
    Ticket ticket = CreateTestTicket();

    if (current >= TicketStatus.Assigned) ticket.ChangeStatus(TicketStatus.Assigned, _now);
    if (current >= TicketStatus.InProgress) ticket.ChangeStatus(TicketStatus.InProgress, _now);
    if (current >= TicketStatus.Resolved) ticket.ChangeStatus(TicketStatus.Resolved, _now);

    Assert.Throws<InvalidOperationException>(() =>
      ticket.ChangeStatus(invalidNext, _now)
    );
  }
}