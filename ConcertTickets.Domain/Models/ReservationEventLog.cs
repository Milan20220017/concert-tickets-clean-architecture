namespace ConcertTickets_API.Domain.Models;

public class ReservationEventLog
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int ReservationId { get; set; }
    public int ConcertId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public int TicketCount { get; set; }
}