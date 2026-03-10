using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _db;
    public ReservationRepository(AppDbContext db) => _db = db;

    public async Task<Reservation?> GetByIdAsync(int id, bool includeItems = false, CancellationToken ct = default)
    {
        var q = _db.Reservations.AsNoTracking().AsQueryable();
        if (includeItems)
            q = q.Include(r => r.Items).ThenInclude(i => i.RegionSeating)
                 .Include(r => r.Currency)
                 .Include(r => r.Concert);

        return await q.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Reservation> AddAsync(Reservation reservation, CancellationToken ct = default)
    {
        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync(ct);
        return reservation;
    }
    public async Task<Reservation?> GetByLoginCodeAsync(string loginCode, bool includeItems = false, CancellationToken ct = default)
    {
        loginCode = (loginCode ?? "").Trim().ToUpperInvariant();

        var q = _db.Reservations.AsNoTracking().AsQueryable();

        if (includeItems)
        {
            q = q
              .Include(r => r.Currency)
              .Include(r => r.Concert!).ThenInclude(c => c.Category)
              .Include(r => r.Concert!).ThenInclude(c => c.Location)
              .Include(r => r.Items).ThenInclude(i => i.RegionSeating)
              .Include(r => r.GeneratedPromoCode)
              .Include(r => r.UsedPromoCode);
        }

        return await q.FirstOrDefaultAsync(r => r.LoginCode == loginCode, ct);
    }
    public async Task<int> GetReservedCountAsync(int concertId, int regionSeatingId, CancellationToken ct = default)
    {
        return await _db.ReservationItems
            .Where(i =>
                i.RegionSeatingId == regionSeatingId &&
                i.Reservation!.ConcertId == concertId &&
                i.Reservation.Status != "Cancelled")
            .SumAsync(i => (int?)i.Quantity, ct) ?? 0;
    }
    public async Task<bool> CancelReservationAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _db.Reservations.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (reservation is null)
            return false;

        if (string.Equals(reservation.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            return true;

        reservation.Status = "Cancelled";
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GetReservedCountExcludingReservationAsync(
    int concertId,
    int regionSeatingId,
    int reservationId,
    CancellationToken ct = default)
    {
        return await _db.ReservationItems
            .Where(i =>
                i.RegionSeatingId == regionSeatingId &&
                i.ReservationId != reservationId &&
                i.Reservation != null &&
                i.Reservation.ConcertId == concertId &&
                i.Reservation.Status != "Cancelled")
            .SumAsync(i => (int?)i.Quantity, ct) ?? 0;
    }

    public async Task<Reservation?> GetByLoginCodeForUpdateAsync(
    string loginCode,
    CancellationToken ct = default)
    {
        loginCode = (loginCode ?? "").Trim().ToUpperInvariant();

        return await _db.Reservations
            .Include(r => r.Items)
                .ThenInclude(i => i.RegionSeating)
            .Include(r => r.Currency)
            .Include(r => r.Concert)
            .Include(r => r.GeneratedPromoCode)
            .Include(r => r.UsedPromoCode)
            .FirstOrDefaultAsync(r => r.LoginCode == loginCode, ct);
    }

    public async Task ReplaceItemsAsync(
    int reservationId,
    List<ReservationItem> newItems,
    CancellationToken ct = default)
    {
        var existingItems = await _db.ReservationItems
            .Where(i => i.ReservationId == reservationId)
            .ToListAsync(ct);

        _db.ReservationItems.RemoveRange(existingItems);

        await _db.ReservationItems.AddRangeAsync(newItems, ct);
    }
    public async Task<bool> ExistsForCurrencyAsync(int currencyId, CancellationToken ct = default)
    {
        return await _db.Reservations.AnyAsync(r => r.CurrencyId == currencyId, ct);
    }

    public async Task<bool> ExistsForRegionAsync(int regionSeatingId, CancellationToken ct = default)
    {
        return await _db.ReservationItems.AnyAsync(i => i.RegionSeatingId == regionSeatingId, ct);
    }

    public async Task<bool> ExistsForConcertAsync(int concertId, CancellationToken ct = default)
    {
        return await _db.Reservations.AnyAsync(r => r.ConcertId == concertId, ct);
    }
}
   

