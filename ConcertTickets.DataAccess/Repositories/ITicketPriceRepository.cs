using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.DataAccess.Repositories;

public interface ITicketPriceRepository
{
    Task<List<TicketPrice>> GetByConcertAsync(int concertId, CancellationToken ct = default);
    Task<TicketPrice> UpsertAsync(TicketPrice price, CancellationToken ct = default);
}
