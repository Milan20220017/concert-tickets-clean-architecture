namespace ConcertTickets_API.DTO;

public class RegionAvailabilityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int ReservedSeats { get; set; }
    public int AvailableSeats { get; set; }
}