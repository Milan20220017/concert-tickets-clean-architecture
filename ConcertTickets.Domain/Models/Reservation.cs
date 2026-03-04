using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcertTickets_API.Domain.Models;

public class Reservation
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } // UTC
    public string Status { get; set; } = "Created"; // Created/Paid/Cancelled...

    public string Email { get; set; } = string.Empty;
    public string LoginCode { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }

    
    public int ConcertId { get; set; }
    public Concert? Concert { get; set; }

    
    public int CurrencyId { get; set; }
    public Currency? Currency { get; set; }
    public decimal DiscountPercentApplied { get; set; } // 0 ili 10
    public ICollection<ReservationItem> Items { get; set; } = new List<ReservationItem>();

    
    public PromoCode? GeneratedPromoCode { get; set; }

    
    public int? UsedPromoCodeId { get; set; }
    public PromoCode? UsedPromoCode { get; set; }
}