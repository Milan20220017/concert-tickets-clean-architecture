namespace ConcertTickets_API.DTO;

public class UpdateReservationRequest
{
    public string LoginCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<UpdateReservationItemRequest> Items { get; set; } = new();
}