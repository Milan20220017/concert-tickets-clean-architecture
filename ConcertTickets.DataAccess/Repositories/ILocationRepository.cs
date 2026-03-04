using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.DataAccess.Repositories;

public interface ILocationRepository
{
    Task<List<Location>> GetAllAsync(CancellationToken ct = default);
    Task<Location?> GetByIdAsync(int id, bool includeRegions = false, CancellationToken ct = default);
    Task<Location> AddAsync(Location location, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

}
