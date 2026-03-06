using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class PromoCodeRepository : IPromoCodeRepository
{
    private readonly AppDbContext _db;
    public PromoCodeRepository(AppDbContext db) => _db = db;

    public Task<PromoCode?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.PromoCodes.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PromoCode?> GetByCreatedByReservationIdAsync(int reservationId, CancellationToken ct = default)
    {
        return await _db.PromoCodes
            .FirstOrDefaultAsync(p => p.CreatedByReservationId == reservationId, ct);
    }
    public Task SaveAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
