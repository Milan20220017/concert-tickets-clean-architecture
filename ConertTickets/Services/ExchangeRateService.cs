using System.Text.Json;

namespace ConcertTickets_API.Services;

public class ExchangeRateService
{
    private readonly HttpClient _httpClient;

    public ExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.frankfurter.dev/v1/");
    }

    public async Task<decimal> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            return amount;

        var url = $"latest?base={from.ToUpperInvariant()}&symbols={to.ToUpperInvariant()}";
        using var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("rates", out var rates))
            throw new ArgumentException("API za kurs nije vratio rates.");

        if (!rates.TryGetProperty(to.ToUpperInvariant(), out var rateElement))
            throw new ArgumentException($"Kurs za valutu {to} nije pronađen.");

        var rate = rateElement.GetDecimal();
        return decimal.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
    }
}