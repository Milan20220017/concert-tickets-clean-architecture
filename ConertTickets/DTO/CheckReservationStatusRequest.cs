namespace ConcertTickets_API.DTO;

public class CheckReservationStatusRequest
{
    public string LoginCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}