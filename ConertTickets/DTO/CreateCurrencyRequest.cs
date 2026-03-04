namespace ConcertTickets_API.DTO;

public class CreateCurrencyRequest
{
    public string Code { get; set; } = string.Empty; // EUR, USD...
}