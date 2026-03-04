using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
        => await _db.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Category> AddAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return false;

        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
