using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcertTickets_API.Domain.Models;

public class Currency
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // EUR, USD, RSD
}
