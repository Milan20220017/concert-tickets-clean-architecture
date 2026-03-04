using ConcertTickets_API.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly AppDbContext _db;
    public CurrencyRepository(AppDbContext db) => _db = db;

    public Task<List<Currency>> GetAllAsync(CancellationToken ct = default)
        => _db.Currencies.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);

    public Task<Currency?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Currency?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _db.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<Currency> AddAsync(Currency c, CancellationToken ct = default)
    {
        _db.Currencies.Add(c);
        await _db.SaveChangesAsync(ct);
        return c;
    }
}
