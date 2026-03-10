namespace ConcertTickets_API.DTO;

public class ReservationBreakdownDto
{
    public decimal SubtotalBeforeDiscounts { get; set; }
    public decimal EarlyBirdDiscountAmount { get; set; }
    public decimal PromoDiscountAmount { get; set; }
    public decimal FinalTotalPrice { get; set; }
    public List<ReservationPriceBreakdownItemDto> PriceBreakdown { get; set; } = new();
}