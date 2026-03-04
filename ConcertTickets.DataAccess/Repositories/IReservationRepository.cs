using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.DataAccess.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id, bool includeItems = false, CancellationToken ct = default);
    Task<Reservation> AddAsync(Reservation reservation, CancellationToken ct = default);

    Task<Reservation?> GetByLoginCodeAsync(string loginCode, bool includeItems = false, CancellationToken ct = default);

    Task<int> GetReservedCountAsync(int concertId, int regionSeatingId, CancellationToken ct = default);
}
