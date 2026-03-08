using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class ReservationRequestStatusRepository : IReservationRequestStatusRepository
{
    private readonly AppDbContext _db;

    public ReservationRequestStatusRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ReservationRequestStatus?> GetByLoginCodeAsync(string loginCode, CancellationToken ct = default)
    {
        return await _db.ReservationRequestStatuses
            .FirstOrDefaultAsync(x => x.LoginCode == loginCode, ct);
    }

    public async Task<ReservationRequestStatus> AddAsync(ReservationRequestStatus entity, CancellationToken ct = default)
    {
        _db.ReservationRequestStatuses.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
