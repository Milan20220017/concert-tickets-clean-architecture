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
                TicketsSold = g.Sum(x =>
                    x.EventType == "ReservationCreated" ? x.TicketCount :
                    x.EventType == "ReservationCancelled" ? -x.TicketCount :
                    0)
            })
            .OrderByDescending(x => x.TicketsSold)
            .ToListAsync(ct);

        return result;
    }

    public async Task<List<LocationSalesReportDto>> GetLocationSalesAsync(CancellationToken ct = default)
    {
        var result = await _db.ReservationEventLogs
            .GroupBy(x => x.ConcertId)
            .Select(g => new LocationSalesReportDto
            {
                ConcertId = g.Key,
                TicketsSold = g.Sum(x =>
                    x.EventType == "ReservationCreated" ? x.TicketCount :
                    x.EventType == "ReservationCancelled" ? -x.TicketCount :
                    0)
            })
            .OrderByDescending(x => x.TicketsSold)
            .ToListAsync(ct);

        return result;
    }
}