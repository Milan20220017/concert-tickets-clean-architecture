using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.DataAccess.Repositories;

public interface IConcertRepository
{
    Task<List<Concert>> GetAllAsync(bool includeRefs = false, CancellationToken ct = default);
    Task<Concert?> GetByIdAsync(int id, bool includeRefs = false, CancellationToken ct = default);
    Task<Concert> AddAsync(Concert concert, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
