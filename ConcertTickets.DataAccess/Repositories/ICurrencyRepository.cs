using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.DataAccess.Repositories;

public interface ICurrencyRepository
{
    Task<List<Currency>> GetAllAsync(CancellationToken ct = default);
    Task<Currency?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Currency?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Currency> AddAsync(Currency c, CancellationToken ct = default);
}
