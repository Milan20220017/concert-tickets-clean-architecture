using ConcertTickets_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public int CreatedByReservationId { get; set; }

    [JsonIgnore]
    public Reservation? CreatedByReservation { get; set; }

    public int? UsedByReservationId { get; set; }

    [JsonIgnore]
    public Reservation? UsedByReservation { get; set; }
}