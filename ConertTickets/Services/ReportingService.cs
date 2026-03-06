using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.DTO;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.Services;

public class ReportingService
{
    private readonly AppDbContext _db;

    public ReportingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ConcertSalesReportDto>> GetConcertSalesAsync(CancellationToken ct = default)
    {
        var result = await _db.ReservationEventLogs
            .Where(e => e.EventType == "ReservationCreated")
            .Join(
                _db.Concerts,
                e => e.ConcertId,
                c => c.Id,
                (e, c) => new { e, c }
            )
            .GroupBy(x => new { x.c.Id, x.c.Name })
            .Select(g => new ConcertSalesReportDto
            {
                ConcertId = g.Key.Id,
                ConcertName = g.Key.Name,
                TicketsSold = g.Sum(x => x.e.TicketCount)
            })
            .OrderByDescending(x => x.TicketsSold)
            .ToListAsync(ct);

        return result;
    }

    public async Task<List<LocationSalesReportDto>> GetLocationSalesAsync(CancellationToken ct = default)
    {
        var result = await _db.ReservationEventLogs
            .Where(e => e.EventType == "ReservationCreated")
            .Join(
                _db.Concerts,
                e => e.ConcertId,
                c => c.Id,
                (e, c) => new { e, c }
            )
            .Join(
                _db.Locations,
                ec => ec.c.LocationId,
                l => l.Id,
                (ec, l) => new { ec.e, l }
            )
            .GroupBy(x => new { x.l.Id, x.l.Name })
            .Select(g => new LocationSalesReportDto
            {
                LocationId = g.Key.Id,
                LocationName = g.Key.Name,
                TicketsSold = g.Sum(x => x.e.TicketCount)
            })
            .OrderByDescending(x => x.TicketsSold)
            .ToListAsync(ct);

        return result;
    }
}
