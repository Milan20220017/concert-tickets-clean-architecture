using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcertTickets_API.Domain.Models;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public ICollection<RegionSeating> Regions { get; set; } = new List<RegionSeating>();//da bi se moglo dohvatiti sve regione neke lokacije
}
