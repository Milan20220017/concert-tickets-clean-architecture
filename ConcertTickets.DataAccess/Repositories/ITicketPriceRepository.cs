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

    Task<bool> ExistsForCurrencyAsync(int currencyId, CancellationToken ct = default);

    Task<bool> ExistsForRegionAsync(int regionSeatingId, CancellationToken ct = default);

    Task<TicketPrice?> GetAsync(
    int concertId,
    int regionSeatingId,
    int currencyId,
    CancellationToken ct = default);
}
