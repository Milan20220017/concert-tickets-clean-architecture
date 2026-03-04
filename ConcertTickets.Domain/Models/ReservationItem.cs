using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;
using ConcertTickets_API.Domain.Models;

public class ReservationItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }

    public int ReservationId { get; set; }

    [JsonIgnore]
    public Reservation? Reservation { get; set; }

    public int RegionSeatingId { get; set; }
    public RegionSeating? RegionSeating { get; set; }
}
