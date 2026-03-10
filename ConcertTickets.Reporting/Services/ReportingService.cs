using ConcertTickets.Reporting.Data;
using ConcertTickets.Reporting.DTO;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets.Reporting.Services;

public class ReportingService
{
    private readonly ReportingDbContext _db;

    public ReportingService(ReportingDbContext db)
    {
        _db = db;
    }

    public async Task<List<ConcertSalesReportDto>> GetConcertSalesAsync(CancellationToken ct = default)
    {
        return await _db.ReservationEventLogs
            .AsNoTracking()
            .GroupBy(x => x.ConcertId)
            .Select(g => new ConcertSalesReportDto
            {
                ConcertId = g.Key,
                CreatedTickets = g.Sum(x => x.EventType == "ReservationCreated" ? x.TicketCount : 0),
                CancelledTickets = g.Sum(x => x.EventType == "ReservationCancelled" ? x.TicketCount : 0),
                NetTicketsSold =
                    g.Sum(x => x.EventType == "ReservationCreated" ? x.TicketCount : 0) -
                    g.Sum(x => x.EventType == "ReservationCancelled" ? x.TicketCount : 0)
            })
            .OrderByDescending(x => x.NetTicketsSold)
            .ThenBy(x => x.ConcertId)
            .ToListAsync(ct);
    }

    public async Task<List<LocationSalesReportDto>> GetLocationSalesAsync(CancellationToken ct = default)
    {
        return await _db.ReservationEventLogs
            .AsNoTracking()
            .GroupBy(x => x.LocationId!)
            .Select(g => new LocationSalesReportDto
            {
                LocationId = g.Key,
                CreatedTickets = g.Sum(x => x.EventType == "ReservationCreated" ? x.TicketCount : 0),
                CancelledTickets = g.Sum(x => x.EventType == "ReservationCancelled" ? x.TicketCount : 0),
                NetTicketsSold =
                    g.Sum(x => x.EventType == "ReservationCreated" ? x.TicketCount : 0) -
                    g.Sum(x => x.EventType == "ReservationCancelled" ? x.TicketCount : 0)
            })
            .OrderByDescending(x => x.NetTicketsSold)
            .ThenBy(x => x.LocationId)
            .ToListAsync(ct);
    }
}