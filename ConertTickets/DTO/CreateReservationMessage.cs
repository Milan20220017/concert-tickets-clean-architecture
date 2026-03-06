namespace ConcertTickets_API.DTO;

public class CreateReservationMessage
{
    public int ConcertId { get; set; }
    public int CurrencyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int? UsedPromoCodeId { get; set; }
    public List<CreateReservationMessageItem> Items { get; set; } = new();
}

public class CreateReservationMessageItem
{
    public int RegionSeatingId { get; set; }
    public int Quantity { get; set; }
}