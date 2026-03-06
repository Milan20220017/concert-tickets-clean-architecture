namespace ConcertTickets_API.DTO;

public class CancelReservationByCodeRequest
{
    public string LoginCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}