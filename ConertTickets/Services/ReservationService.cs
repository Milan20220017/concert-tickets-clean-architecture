using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;
using ConcertTickets_API.DTO;

namespace ConcertTickets_API.Services;

public class ReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly IConcertRepository _concerts;
    private readonly ICurrencyRepository _currencies;
    private readonly ITicketPriceRepository _prices;
    private readonly IPromoCodeRepository _promoCodes;
    private readonly IRegionSeatingRepository _regions;
    private readonly ExchangeRateService _exchangeRateService;

    public ReservationService(
       IReservationRepository reservations,
       IConcertRepository concerts,
       IRegionSeatingRepository regions,
       ICurrencyRepository currencies,
       ITicketPriceRepository prices,
       IPromoCodeRepository promoCodes,
       ExchangeRateService exchangeRateService)
    {
        _reservations = reservations;
        _concerts = concerts;
        _currencies = currencies;
        _prices = prices;
        _promoCodes = promoCodes;
        _regions = regions;
        _exchangeRateService = exchangeRateService;
    }

    public Task<Reservation?> GetByLoginCodeAsync(string loginCode, CancellationToken ct = default)
        => _reservations.GetByLoginCodeAsync(loginCode, includeItems: true, ct);

    public Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default)
        => _reservations.GetByIdAsync(id, includeItems: true, ct);

    public async Task<Reservation> CreateAsync(
     string loginCode,
     int concertId,
     int currencyId,
     string email,
     List<(int RegionSeatingId, int Quantity)> items,
     string? promoCode,
     CancellationToken ct = default)
    {
        email = (email ?? "").Trim();
        if (email.Length < 5)
            throw new ArgumentException("Email nije validan.");

        if (items is null || items.Count == 0)
            throw new ArgumentException("Moraš dodati bar jednu stavku.");

        var concert = await _concerts.GetByIdAsync(concertId, includeRefs: false, ct);
        if (concert is null)
            throw new ArgumentException("Koncert ne postoji.");

        var selectedCurrency = await _currencies.GetByIdAsync(currencyId, ct);
        if (selectedCurrency is null)
            throw new ArgumentException("Valuta ne postoji.");

        var baseCurrency = await _currencies.GetByCodeAsync("EUR", ct);
        if (baseCurrency is null)
            throw new ArgumentException("Bazna valuta EUR nije definisana.");

        var allPrices = await _prices.GetByConcertAsync(concertId, ct);

        var basePriceMap = allPrices
            .Where(p => p.CurrencyId == baseCurrency.Id)
            .ToDictionary(p => p.RegionSeatingId, p => p.Amount);

        var concertUtc = concert.Date;
        if (concertUtc.Kind == DateTimeKind.Unspecified)
            concertUtc = DateTime.SpecifyKind(concertUtc, DateTimeKind.Local).ToUniversalTime();
        else
            concertUtc = concertUtc.ToUniversalTime();

        DateTime? earlyBirdUntilUtc = null;

        if (concert.EarlyBirdDiscountUntil.HasValue)
        {
            var earlyBirdValue = concert.EarlyBirdDiscountUntil.Value;

            if (earlyBirdValue.Kind == DateTimeKind.Unspecified)
                earlyBirdUntilUtc = DateTime.SpecifyKind(earlyBirdValue, DateTimeKind.Local).ToUniversalTime();
            else
                earlyBirdUntilUtc = earlyBirdValue.ToUniversalTime();
        }

        bool earlyBirdActive =
            earlyBirdUntilUtc.HasValue &&
            DateTime.UtcNow <= earlyBirdUntilUtc.Value;

        decimal earlyBirdMultiplier = earlyBirdActive ? 0.9m : 1m;

        PromoCode? promo = null;
        bool promoApplied = false;

        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            promo = await _promoCodes.GetByCodeAsync(promoCode.Trim().ToUpperInvariant(), ct);
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
            LoginCode = loginCode,
            Status = "Created",
            UsedPromoCodeId = promo?.Id,
            DiscountPercentApplied = earlyBirdActive ? 10m : 0m
        };

        decimal total = 0m;

        foreach (var (regionId, qty) in items)
        {
            if (qty <= 0)
                throw new ArgumentException("Količina mora biti > 0.");

            var region = await _regions.GetByIdAsync(regionId, ct);
            if (region is null)
                throw new ArgumentException("Region sjedenja ne postoji.");

            var alreadyReserved = await _reservations.GetReservedCountAsync(concertId, regionId, ct);
            if (alreadyReserved + qty > region.Capacity)
            {
                var remaining = region.Capacity - alreadyReserved;
                throw new ArgumentException($"Nema dovoljno mjesta u regionu '{region.Name}'. Preostalo: {remaining}.");
            }

            if (!basePriceMap.TryGetValue(regionId, out var baseUnitPrice))
                throw new ArgumentException("Nema bazne cijene za izabrani region.");

            var discountedBasePrice = baseUnitPrice * earlyBirdMultiplier;

            var convertedUnitPrice = await _exchangeRateService.ConvertAsync(
                baseCurrency.Code,
                selectedCurrency.Code,
                discountedBasePrice,
                ct
            );

            total += convertedUnitPrice * qty;

            reservation.Items.Add(new ReservationItem
            {
                RegionSeatingId = regionId,
                Quantity = qty
            });
        }

        if (promoApplied)
            total *= 0.95m;

        reservation.TotalPrice = decimal.Round(total, 2, MidpointRounding.AwayFromZero);

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

    public async Task<bool> CancelReservationAsync(int reservationId, CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByIdAsync(reservationId, includeItems: false, ct);
        if (reservation is null)
            return false;

        if (string.Equals(reservation.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            return true;

        var concert = await _concerts.GetByIdAsync(reservation.ConcertId, includeRefs: false, ct);
        if (concert is null)
            throw new ArgumentException("Koncert ne postoji.");

        var concertUtc = concert.Date;
        if (concertUtc.Kind == DateTimeKind.Unspecified)
            concertUtc = DateTime.SpecifyKind(concertUtc, DateTimeKind.Local).ToUniversalTime();
        else
            concertUtc = concertUtc.ToUniversalTime();

        if (DateTime.UtcNow >= concertUtc)
            throw new ArgumentException("Rezervaciju nije moguće otkazati nakon početka koncerta.");

        var generatedPromo = await _promoCodes.GetByCreatedByReservationIdAsync(reservationId, ct);
        if (generatedPromo is not null &&
            string.Equals(generatedPromo.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            generatedPromo.Status = "Inactive";
            await _promoCodes.SaveAsync(ct);
        }

        return await _reservations.CancelReservationAsync(reservationId, ct);
    }
    public async Task<Reservation?> GetByLoginCodeAndEmailAsync(string loginCode, string email, CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByLoginCodeForUpdateAsync(loginCode, ct);

        if (reservation is null)
            return null;

        if (!string.Equals(reservation.Email?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase))
            return null;

        return reservation;
    }

    public async Task<Reservation> UpdateReservationByCodeAsync(
    string loginCode,
    string email,
    List<(int RegionSeatingId, int Quantity)> items,
    CancellationToken ct = default)
    {
        loginCode = (loginCode ?? "").Trim().ToUpperInvariant();
        email = (email ?? "").Trim();

        if (string.IsNullOrWhiteSpace(loginCode) || string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Login code i email su obavezni.");

        if (items is null || items.Count == 0)
            throw new ArgumentException("Morate poslati bar jednu stavku.");

        var reservation = await _reservations.GetByLoginCodeForUpdateAsync(loginCode, ct);
        if (reservation is null)
            throw new ArgumentException("Rezervacija ne postoji.");

        if (!string.Equals(reservation.Email?.Trim(), email, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Rezervacija sa datom šifrom i email adresom ne postoji.");

        if (string.Equals(reservation.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Otkazana rezervacija se ne može mijenjati.");

        var concert = await _concerts.GetByIdAsync(reservation.ConcertId, includeRefs: false, ct);
        if (concert is null)
            throw new ArgumentException("Koncert ne postoji.");

        var concertUtc = concert.Date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(concert.Date, DateTimeKind.Local).ToUniversalTime()
            : concert.Date.ToUniversalTime();

        if (DateTime.UtcNow >= concertUtc)
            throw new ArgumentException("Rezervaciju nije moguće mijenjati nakon početka koncerta.");

        var selectedCurrency = await _currencies.GetByIdAsync(reservation.CurrencyId, ct);
        if (selectedCurrency is null)
            throw new ArgumentException("Valuta rezervacije ne postoji.");

        var baseCurrency = await _currencies.GetByCodeAsync("EUR", ct);
        if (baseCurrency is null)
            throw new ArgumentException("Bazna valuta EUR nije definisana.");

        var allPrices = await _prices.GetByConcertAsync(reservation.ConcertId, ct);

        var basePriceMap = allPrices
            .Where(p => p.CurrencyId == baseCurrency.Id)
            .ToDictionary(p => p.RegionSeatingId, p => p.Amount);

        bool earlyBirdApplied = reservation.DiscountPercentApplied > 0;
        decimal earlyBirdMultiplier = earlyBirdApplied ? 0.9m : 1m;

        bool promoApplied = reservation.UsedPromoCodeId.HasValue;

        decimal total = 0m;
        var newItems = new List<ReservationItem>();

        foreach (var (regionId, qty) in items)
        {
            if (qty <= 0)
                throw new ArgumentException("Količina mora biti > 0.");

            var region = await _regions.GetByIdAsync(regionId, ct);
            if (region is null)
                throw new ArgumentException("Region sjedenja ne postoji.");

            var reservedExcludingThis = await _reservations.GetReservedCountExcludingReservationAsync(
                reservation.ConcertId,
                regionId,
                reservation.Id,
                ct
            );

            if (reservedExcludingThis + qty > region.Capacity)
            {
                var remaining = region.Capacity - reservedExcludingThis;
                throw new ArgumentException($"Nema dovoljno mjesta u regionu '{region.Name}'. Preostalo: {remaining}.");
            }

            if (!basePriceMap.TryGetValue(regionId, out var baseUnitPrice))
                throw new ArgumentException("Nema bazne cijene za izabrani region.");

            var discountedBasePrice = baseUnitPrice * earlyBirdMultiplier;

            var convertedUnitPrice = await _exchangeRateService.ConvertAsync(
                baseCurrency.Code,
                selectedCurrency.Code,
                discountedBasePrice,
                ct
            );

            total += convertedUnitPrice * qty;

            newItems.Add(new ReservationItem
            {
                ReservationId = reservation.Id,
                RegionSeatingId = regionId,
                Quantity = qty
            });
        }

        if (promoApplied)
            total *= 0.95m;

        reservation.TotalPrice = decimal.Round(total, 2, MidpointRounding.AwayFromZero);

        await _reservations.ReplaceItemsAsync(reservation.Id, newItems, ct);
        await _reservations.SaveAsync(ct);

        var refreshed = await _reservations.GetByLoginCodeAsync(loginCode, includeItems: true, ct);
        return refreshed ?? reservation;
    }
    public async Task<ReservationBreakdownDto> BuildReservationBreakdownAsync(
    Reservation reservation,
    CancellationToken ct = default)
    {
        var baseCurrency = await _currencies.GetByCodeAsync("EUR", ct);
        if (baseCurrency is null)
            throw new ArgumentException("Bazna valuta EUR nije definisana.");

        var selectedCurrency = await _currencies.GetByIdAsync(reservation.CurrencyId, ct);
        if (selectedCurrency is null)
            throw new ArgumentException("Valuta rezervacije ne postoji.");

        var allPrices = await _prices.GetByConcertAsync(reservation.ConcertId, ct);

        var eurPrices = allPrices
            .Where(p => p.CurrencyId == baseCurrency.Id)
            .ToDictionary(p => p.RegionSeatingId, p => p.Amount);

        var breakdown = new List<ReservationPriceBreakdownItemDto>();
        decimal subtotalBeforeDiscounts = 0m;
        decimal subtotalAfterEarlyBird = 0m;

        bool earlyBirdApplied = reservation.DiscountPercentApplied > 0;
        decimal earlyBirdMultiplier = earlyBirdApplied ? 0.9m : 1m;

        foreach (var item in reservation.Items)
        {
            if (!eurPrices.TryGetValue(item.RegionSeatingId, out var eurUnitPrice))
                throw new ArgumentException("Nema bazne EUR cijene za region.");

            var convertedBaseUnitPrice = await _exchangeRateService.ConvertAsync(
                baseCurrency.Code,
                selectedCurrency.Code,
                eurUnitPrice,
                ct
            );

            var convertedDiscountedUnitPrice = await _exchangeRateService.ConvertAsync(
                baseCurrency.Code,
                selectedCurrency.Code,
                eurUnitPrice * earlyBirdMultiplier,
                ct
            );

            var baseLineTotal = convertedBaseUnitPrice * item.Quantity;
            var discountedLineTotal = convertedDiscountedUnitPrice * item.Quantity;

            subtotalBeforeDiscounts += baseLineTotal;
            subtotalAfterEarlyBird += discountedLineTotal;

            breakdown.Add(new ReservationPriceBreakdownItemDto
            {
                RegionSeatingId = item.RegionSeatingId,
                RegionName = item.RegionSeating?.Name ?? $"Region {item.RegionSeatingId}",
                Quantity = item.Quantity,
                UnitPrice = decimal.Round(convertedDiscountedUnitPrice, 2, MidpointRounding.AwayFromZero),
                LineTotal = decimal.Round(discountedLineTotal, 2, MidpointRounding.AwayFromZero)
            });
        }

        var earlyBirdDiscountAmount = subtotalBeforeDiscounts - subtotalAfterEarlyBird;

        decimal promoDiscountAmount = 0m;
        decimal finalTotal = subtotalAfterEarlyBird;

        if (reservation.UsedPromoCodeId.HasValue)
        {
            promoDiscountAmount = subtotalAfterEarlyBird * 0.05m;
            finalTotal -= promoDiscountAmount;
        }

        return new ReservationBreakdownDto
        {
            SubtotalBeforeDiscounts = decimal.Round(subtotalBeforeDiscounts, 2, MidpointRounding.AwayFromZero),
            EarlyBirdDiscountAmount = decimal.Round(earlyBirdDiscountAmount, 2, MidpointRounding.AwayFromZero),
            PromoDiscountAmount = decimal.Round(promoDiscountAmount, 2, MidpointRounding.AwayFromZero),
            FinalTotalPrice = decimal.Round(finalTotal, 2, MidpointRounding.AwayFromZero),
            PriceBreakdown = breakdown
        };
    }
    public async Task<CalculateReservationResponse> CalculateAsync(
    CalculateReservationRequest req,
    CancellationToken ct)
    {
        if (req.Items is null || req.Items.Count == 0)
            throw new ArgumentException("Morate poslati bar jednu stavku.");

        var concert = await _concerts.GetByIdAsync(req.ConcertId, false, ct);
        if (concert is null)
            throw new ArgumentException("Concert not found.");

        var selectedCurrency = await _currencies.GetByIdAsync(req.CurrencyId, ct);
        if (selectedCurrency is null)
            throw new ArgumentException("Selected currency not found.");

        var baseCurrency = await _currencies.GetByCodeAsync("EUR", ct);
        if (baseCurrency is null)
            throw new ArgumentException("Base currency EUR is not defined.");

        var allPrices = await _prices.GetByConcertAsync(req.ConcertId, ct);

        var basePriceMap = allPrices
            .Where(p => p.CurrencyId == baseCurrency.Id)
            .ToDictionary(p => p.RegionSeatingId, p => p);

        DateTime? earlyBirdUntilUtc = null;
        if (concert.EarlyBirdDiscountUntil.HasValue)
        {
            var eb = concert.EarlyBirdDiscountUntil.Value;
            earlyBirdUntilUtc = eb.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(eb, DateTimeKind.Local).ToUniversalTime()
                : eb.ToUniversalTime();
        }

        bool earlyBirdActive =
            earlyBirdUntilUtc.HasValue &&
            DateTime.UtcNow <= earlyBirdUntilUtc.Value;

        decimal subtotal = 0m;
        var breakdown = new List<PriceBreakdownItem>();

        foreach (var item in req.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            if (!basePriceMap.TryGetValue(item.RegionSeatingId, out var basePrice))
                throw new ArgumentException($"Base EUR ticket price not found for region {item.RegionSeatingId}.");

            decimal eurUnitPrice = basePrice.Amount;

            decimal convertedUnitPrice = await _exchangeRateService.ConvertAsync(
                baseCurrency.Code,
                selectedCurrency.Code,
                eurUnitPrice,
                ct
            );

            decimal lineTotal = convertedUnitPrice * item.Quantity;
            subtotal += lineTotal;

            breakdown.Add(new PriceBreakdownItem
            {
                RegionName = basePrice.RegionSeating?.Name ?? $"Region {item.RegionSeatingId}",
                Quantity = item.Quantity,
                UnitPrice = decimal.Round(convertedUnitPrice, 2, MidpointRounding.AwayFromZero),
                LineTotal = decimal.Round(lineTotal, 2, MidpointRounding.AwayFromZero)
            });
        }

        decimal earlyBirdDiscount = 0m;
        if (earlyBirdActive)
            earlyBirdDiscount = subtotal * 0.10m;

        decimal promoDiscount = 0m;
        if (!string.IsNullOrWhiteSpace(req.PromoCode))
        {
            var promo = await _promoCodes.GetByCodeAsync(req.PromoCode.Trim().ToUpperInvariant(), ct);

            if (promo is null)
                throw new ArgumentException("Promo code does not exist.");

            if (!string.Equals(promo.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Promo code is not active.");

            if (promo.UsedByReservationId is not null)
                throw new ArgumentException("Promo code has already been used.");

            promoDiscount = (subtotal - earlyBirdDiscount) * 0.05m;
        }

        var finalTotal = subtotal - earlyBirdDiscount - promoDiscount;

        return new CalculateReservationResponse
        {
            Subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            EarlyBirdDiscount = decimal.Round(earlyBirdDiscount, 2, MidpointRounding.AwayFromZero),
            PromoDiscount = decimal.Round(promoDiscount, 2, MidpointRounding.AwayFromZero),
            FinalTotal = decimal.Round(finalTotal, 2, MidpointRounding.AwayFromZero),
            Breakdown = breakdown
        };
    }
}