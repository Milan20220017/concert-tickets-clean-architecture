using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;

namespace ConcertTickets_API.Services;

public class CategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly IConcertRepository _concerts;

    public CategoryService(ICategoryRepository repo, IConcertRepository concerts) =>
        (_repo, _concerts) = (repo, concerts);

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

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var concerts = await _concerts.GetFilteredAsync(
            includeRefs: false,
            categoryId: id,
            locationId: null,
            dateFrom: null,
            dateTo: null,
            onlyPublished: false,
            ct
        );

        if (concerts.Any())
            throw new ArgumentException("Kategorija se ne može obrisati jer postoje koncerti koji koriste ovu kategoriju.");

        return await _repo.DeleteAsync(id, ct);
    }
}

