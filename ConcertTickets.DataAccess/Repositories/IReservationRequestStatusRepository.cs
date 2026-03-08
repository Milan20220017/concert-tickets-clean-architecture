using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.DataAccess.Repositories;

public interface IReservationRequestStatusRepository
{
    Task<ReservationRequestStatus?> GetByLoginCodeAsync(string loginCode, CancellationToken ct = default);
    Task<ReservationRequestStatus> AddAsync(ReservationRequestStatus entity, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
