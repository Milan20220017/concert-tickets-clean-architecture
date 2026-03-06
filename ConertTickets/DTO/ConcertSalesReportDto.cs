namespace ConcertTickets_API.DTO;

public class ConcertSalesReportDto
{
    public int ConcertId { get; set; }
    public string ConcertName { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
}
