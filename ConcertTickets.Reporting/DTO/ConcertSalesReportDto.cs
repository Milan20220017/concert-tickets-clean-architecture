namespace ConcertTickets.Reporting.DTO;

public class ConcertSalesReportDto
{
    public int ConcertId { get; set; }
    public int CreatedTickets { get; set; }
    public int CancelledTickets { get; set; }
    public int NetTicketsSold { get; set; }
}