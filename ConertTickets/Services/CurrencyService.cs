using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Net.Http;

namespace ConcertTickets_API.Services;

public class CurrencyService
{
    private readonly ICurrencyRepository _repo;
    private readonly ITicketPriceRepository _prices;
    private readonly IReservationRepository _reservations;
    private readonly HttpClient _http;

    public CurrencyService(
        ICurrencyRepository repo,
        ITicketPriceRepository prices,
        IReservationRepository reservation,
        IHttpClientFactory httpFactory)
    {
        _repo = repo;
        _prices = prices;
        _reservations = reservation;
        _http = httpFactory.CreateClient();
    }
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

        // Frankfurter API
        var response = await _http.GetAsync("https://api.frankfurter.app/currencies", ct);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Greška pri pozivu Frankfurter currency API-ja.");

        var json = await response.Content.ReadAsStringAsync(ct);

        var currencies = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        if (currencies == null || !currencies.ContainsKey(code))
            throw new ArgumentException($"Valuta '{code}' ne postoji prema Frankfurter API-ju.");

        return await _repo.AddAsync(new Currency { Code = code }, ct);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var usedInPrices = await _prices.ExistsForCurrencyAsync(id, ct);
        if (usedInPrices)
            throw new ArgumentException("Valuta se ne može obrisati jer je korištena u cijenama karata.");

        var usedInReservations = await _reservations.ExistsForCurrencyAsync(id, ct);
        if (usedInReservations)
            throw new ArgumentException("Valuta se ne može obrisati jer postoje rezervacije u toj valuti.");

        return await _repo.DeleteAsync(id, ct);
    }

}
