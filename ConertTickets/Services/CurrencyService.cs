using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class CurrencyService
{
    private readonly ICurrencyRepository _repo;

    public CurrencyService(ICurrencyRepository repo) => _repo = repo;

    public Task<List<Currency>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<Currency?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<Currency> CreateAsync(string code, CancellationToken ct = default)
    {
        code = (code ?? "").Trim().ToUpperInvariant();
        if (code.Length < 3 || code.Length > 5)
            throw new ArgumentException("Kod valute mora imati 3-5 karaktera (npr. EUR).");

        var existing = await _repo.GetByCodeAsync(code, ct);
        if (existing is not null)
            throw new ArgumentException("Valuta sa tim kodom već postoji.");

        return await _repo.AddAsync(new Currency { Code = code }, ct);
    }
}
