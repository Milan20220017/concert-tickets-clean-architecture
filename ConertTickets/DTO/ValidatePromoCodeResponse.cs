namespace ConcertTickets_API.DTO;

public class ValidatePromoCodeResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? DiscountPercent { get; set; }
}