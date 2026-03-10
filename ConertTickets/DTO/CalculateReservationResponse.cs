public class CalculateReservationResponse
{
    public decimal Subtotal { get; set; }
    public decimal EarlyBirdDiscount { get; set; }
    public decimal PromoDiscount { get; set; }
    public decimal FinalTotal { get; set; }

    public List<PriceBreakdownItem> Breakdown { get; set; } = new();
}

public class PriceBreakdownItem
{
    public string RegionName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}