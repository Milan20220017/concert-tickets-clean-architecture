using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class ReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly IConcertRepository _concerts;
    private readonly ICurrencyRepository _currencies;
    private readonly ITicketPriceRepository _prices;
    private readonly IPromoCodeRepository _promoCodes;
    private readonly IRegionSeatingRepository _regions;

    public ReservationService(
        IReservationRepository reservations,
        IConcertRepository concerts,
        IRegionSeatingRepository regions,
        ICurrencyRepository currencies,
        ITicketPriceRepository prices,
        IPromoCodeRepository promoCodes)
    {
        _reservations = reservations;
        _concerts = concerts;
        _currencies = currencies;
        _prices = prices;
        _promoCodes = promoCodes;
        _regions = regions;
    }

    public Task<Reservation?> GetByLoginCodeAsync(string loginCode, CancellationToken ct = default)
        => _reservations.GetByLoginCodeAsync(loginCode, includeItems: true, ct);

    public async Task<Reservation> CreateAsync(
        int concertId,
        int currencyId,
        string email,
        int? usedPromoCodeId,
        List<(int regionId, int qty)> items,
        CancellationToken ct = default)
    {
        email = (email ?? "").Trim();
        if (email.Length < 5) throw new ArgumentException("Email nije validan.");
        if (items is null || items.Count == 0) throw new ArgumentException("Moraš dodati bar jednu stavku.");

        if (usedPromoCodeId.HasValue && usedPromoCodeId.Value <= 0)
            usedPromoCodeId = null;

        var concert = await _concerts.GetByIdAsync(concertId, includeRefs: false, ct);
        if (concert is null) throw new ArgumentException("Koncert ne postoji.");

        var currency = await _currencies.GetByIdAsync(currencyId, ct);
        if (currency is null) throw new ArgumentException("Valuta ne postoji.");

        var allPrices = await _prices.GetByConcertAsync(concertId, ct);
        var priceMap = allPrices
            .Where(p => p.CurrencyId == currencyId)
            .ToDictionary(p => p.RegionSeatingId, p => p.Amount);

        var concertUtc = concert.Date;
        if (concertUtc.Kind == DateTimeKind.Unspecified)
            concertUtc = DateTime.SpecifyKind(concertUtc, DateTimeKind.Local).ToUniversalTime();
        else
            concertUtc = concertUtc.ToUniversalTime();

        var discountUntil = concertUtc.AddDays(-5);
        bool earlyBirdActive = DateTime.UtcNow <= discountUntil;
        decimal earlyBirdMultiplier = earlyBirdActive ? 0.9m : 1m;

        // PROMO kod (dodatnih 10% popusta na total)
        PromoCode? promo = null;
        bool promoApplied = false;

        if (usedPromoCodeId.HasValue)
        {
            promo = await _promoCodes.GetByIdAsync(usedPromoCodeId.Value, ct);
            if (promo is null)
                throw new ArgumentException("Promo kod ne postoji.");

            if (!string.Equals(promo.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Promo kod nije aktivan.");

            if (promo.UsedByReservationId is not null)
                throw new ArgumentException("Promo kod je već iskorišten.");

            promoApplied = true;
        }

        var reservation = new Reservation
        {
            ConcertId = concertId,
            CurrencyId = currencyId,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            LoginCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            Status = "Created",
            UsedPromoCodeId = usedPromoCodeId,
            DiscountPercentApplied = earlyBirdActive ? 10m : 0m
        };

        decimal total = 0m;

        foreach (var (regionId, qty) in items)
        {
            if (qty <= 0) throw new ArgumentException("Količina mora biti > 0.");

            var region = await _regions.GetByIdAsync(regionId, ct);
            if (region is null)
                throw new ArgumentException("Region sjedenja ne postoji.");

            // capacity check
            var alreadyReserved = await _reservations.GetReservedCountAsync(concertId, regionId, ct);
            if (alreadyReserved + qty > region.Capacity)
            {
                var remaining = region.Capacity - alreadyReserved;
                throw new ArgumentException($"Nema dovoljno mjesta u regionu '{region.Name}'. Preostalo: {remaining}.");
            }

            if (!priceMap.TryGetValue(regionId, out var baseUnitPrice))
                throw new ArgumentException("Nema cijene za izabrani region u ovoj valuti.");

            var finalUnitPrice = baseUnitPrice * earlyBirdMultiplier;
            total += finalUnitPrice * qty;

            reservation.Items.Add(new ReservationItem
            {
                RegionSeatingId = regionId,
                Quantity = qty
            });
        }

        if (promoApplied)
            total *= 0.9m;

        reservation.TotalPrice = total;

        reservation.GeneratedPromoCode = new PromoCode
        {
            Code = $"PROMO-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            Status = "Active",
            CreatedByReservation = reservation
        };

        var saved = await _reservations.AddAsync(reservation, ct);

        if (promoApplied && promo is not null)
        {
            promo.Status = "Used";
            promo.UsedByReservationId = saved.Id;
            await _promoCodes.SaveAsync(ct);
        }

        return saved;
    }
}