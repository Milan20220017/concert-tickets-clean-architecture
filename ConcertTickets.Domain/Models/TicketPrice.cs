using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcertTickets_API.Domain.Models;

public class TicketPrice
{
    public int Id { get; set; }
    public decimal Amount { get; set; }

    public int ConcertId { get; set; }
    public Concert? Concert { get; set; }

    public int RegionSeatingId { get; set; }
    public RegionSeating? RegionSeating { get; set; }

    public int CurrencyId { get; set; }
    public Currency? Currency { get; set; }
}
