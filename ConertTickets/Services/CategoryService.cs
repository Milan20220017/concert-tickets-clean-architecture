using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class CategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public Task<List<Category>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<Category> CreateAsync(string name, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();
        if (name.Length < 2) throw new ArgumentException("Naziv kategorije je prekratak.");

        var category = new Category { Name = name };
        return await _repo.AddAsync(category, ct);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteAsync(id, ct);
}

