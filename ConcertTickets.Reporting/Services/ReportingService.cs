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
        var result = await _db.ReservationEventLogs
            .GroupBy(x => x.ConcertId)
            .Select(g => new ConcertSalesReportDto
            {
                ConcertId = g.Key,
                CreatedTickets = g
                    .Where(x => x.EventType == "ReservationCreated")
                    .Sum(x => x.TicketCount),

                CancelledTickets = g
                    .Where(x => x.EventType == "ReservationCancelled")
                    .Sum(x => x.TicketCount),

                NetTicketsSold =
                    g.Where(x => x.EventType == "ReservationCreated").Sum(x => x.TicketCount)
                    - g.Where(x => x.EventType == "ReservationCancelled").Sum(x => x.TicketCount)
            })
            .OrderByDescending(x => x.NetTicketsSold)
            .ToListAsync(ct);

        return result;
    }

    public async Task<List<LocationSalesReportDto>> GetLocationSalesAsync(CancellationToken ct = default)
    {
        var result = await _db.ReservationEventLogs
            .GroupBy(x => x.LocationId)
            .Select(g => new LocationSalesReportDto
            {
                LocationId = g.Key,
                CreatedTickets = g
                    .Where(x => x.EventType == "ReservationCreated")
                    .Sum(x => x.TicketCount),

                CancelledTickets = g
                    .Where(x => x.EventType == "ReservationCancelled")
                    .Sum(x => x.TicketCount),

                NetTicketsSold =
                    g.Where(x => x.EventType == "ReservationCreated").Sum(x => x.TicketCount)
                    - g.Where(x => x.EventType == "ReservationCancelled").Sum(x => x.TicketCount)
            })
            .OrderByDescending(x => x.NetTicketsSold)
            .ToListAsync(ct);

        return result;
    }
}