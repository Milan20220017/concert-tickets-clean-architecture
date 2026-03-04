namespace ConcertTickets_API.DTO;

public class UpsertTicketPriceRequest
{
    public int ConcertId { get; set; }
    public int RegionSeatingId { get; set; }
    public int CurrencyId { get; set; }
    public decimal Amount { get; set; }
}