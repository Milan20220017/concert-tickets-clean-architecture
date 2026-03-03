using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcertTickets_API.Domain.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
