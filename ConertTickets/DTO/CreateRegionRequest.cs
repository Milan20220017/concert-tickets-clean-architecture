namespace ConcertTickets_API.DTO;

public class CreateRegionRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}