namespace ConcertTickets_API.DTO;

public class LocationSalesReportDto
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
}
