namespace ConcertTickets_API.DTO;

public class CheckReservationStatusResponse
{
    public string LoginCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public int? ReservationId { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? ReservationStatus { get; set; }
    public string? GeneratedPromoCode { get; set; }

    public List<ReservationStatusItemDto> Items { get; set; } = new();

    public decimal? SubtotalBeforeDiscounts { get; set; }
    public decimal? EarlyBirdDiscountAmount { get; set; }
    public decimal? PromoDiscountAmount { get; set; }
    public decimal? FinalTotalPrice { get; set; }

    public List<ReservationPriceBreakdownItemDto> PriceBreakdown { get; set; } = new();
}