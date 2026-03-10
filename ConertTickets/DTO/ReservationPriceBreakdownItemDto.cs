namespace ConcertTickets_API.DTO;

public class ReservationPriceBreakdownItemDto
{
    public int RegionSeatingId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
