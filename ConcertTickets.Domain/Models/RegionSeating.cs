using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcertTickets_API.Domain.Models;

public class RegionSeating
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int LocationId { get; set; }
    public Location? Location { get; set; }
}
