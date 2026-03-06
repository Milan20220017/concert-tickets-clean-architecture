namespace ConcertTickets_API.DTO;

public class ConcertFilterRequest
{
    public int? CategoryId { get; set; }
    public int? LocationId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
