namespace ConcertTickets_API.DTO;

public class CheckReservationStatusResponse
{
    public string LoginCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public int? ReservationId { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? ReservationStatus { get; set; }
}