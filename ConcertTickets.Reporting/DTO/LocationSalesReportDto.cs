namespace ConcertTickets.Reporting.DTO;

public class LocationSalesReportDto
{
    public int LocationId { get; set; }
    public int CreatedTickets { get; set; }
    public int CancelledTickets { get; set; }
    public int NetTicketsSold { get; set; }
}