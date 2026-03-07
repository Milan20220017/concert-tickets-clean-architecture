namespace ConcertTickets_API.DTO;

public class ReservationEventMessage
{
    public string EventType { get; set; } = string.Empty;
    public string ReservationCode { get; set; } = string.Empty;
    public int ConcertId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public int TicketCount { get; set; }

    public int LocationId { get; set; }
}