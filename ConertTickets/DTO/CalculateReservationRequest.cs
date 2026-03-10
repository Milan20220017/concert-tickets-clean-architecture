public class CalculateReservationRequest
{
    public int ConcertId { get; set; }
    public int CurrencyId { get; set; }
    public string? PromoCode { get; set; }

    public List<CalculateReservationItem> Items { get; set; } = new();
}

public class CalculateReservationItem
{
    public int RegionSeatingId { get; set; }
    public int Quantity { get; set; }
}
