using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcertTickets_API.Domain.Models;

public class ReservationRequestStatus
{
    public int Id { get; set; }

    public string LoginCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ConcertId { get; set; }

    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
