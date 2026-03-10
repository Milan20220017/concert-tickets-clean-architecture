namespace ConcertTickets_API.DTO;

public class ReservationStatusItemDto
{
    public int RegionSeatingId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}