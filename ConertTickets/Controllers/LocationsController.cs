using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly LocationService _service;
    public LocationsController(LocationService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] bool includeRegions = false, CancellationToken ct = default)
    {
        var loc = await _service.GetByIdAsync(id, includeRegions, ct);
        return loc is null ? NotFound() : Ok(loc);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLocationRequest req, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(req.Name, req.Address, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
