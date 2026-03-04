namespace ConcertTickets_API.DTO;

public class CreateReservationRequest
{
    public int ConcertId { get; set; }
    public int CurrencyId { get; set; }
    public string Email { get; set; } = string.Empty;

    public int? UsedPromoCodeId { get; set; } // opciono

    public List<CreateReservationItemRequest> Items { get; set; } = new();
}

public class CreateReservationItemRequest
{
    public int RegionSeatingId { get; set; }
    public int Quantity { get; set; }
}
