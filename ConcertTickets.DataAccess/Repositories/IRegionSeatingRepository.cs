using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.DataAccess.Repositories;

public interface IRegionSeatingRepository
{
    Task<List<RegionSeating>> GetByLocationAsync(int locationId, CancellationToken ct = default);
    Task<RegionSeating> AddAsync(RegionSeating region, CancellationToken ct = default);

    Task<RegionSeating?> GetByIdAsync(int id, CancellationToken ct = default);
}
