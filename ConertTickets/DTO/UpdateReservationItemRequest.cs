namespace ConcertTickets_API.DTO;

public class UpdateReservationItemRequest
{
    public int RegionSeatingId { get; set; }
    public int Quantity { get; set; }
}