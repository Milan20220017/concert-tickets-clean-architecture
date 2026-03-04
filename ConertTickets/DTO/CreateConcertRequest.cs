namespace ConcertTickets_API.DTO;

public class CreateConcertRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public int LocationId { get; set; }
}
