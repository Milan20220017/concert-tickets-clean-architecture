using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConcertsController : ControllerBase
{
    private readonly ConcertService _service;
    public ConcertsController(ConcertService service) => _service = service;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] bool includeRefs = false, CancellationToken ct = default)
    {
        var c = await _service.GetByIdAsync(id, includeRefs, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConcertRequest req, CancellationToken ct = default)
    {
        try
        {
            var created = await _service.CreateAsync(req.Name, req.Date, req.CategoryId, req.LocationId, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(
     [FromQuery] bool includeRefs = false,
     [FromQuery] int? categoryId = null,
     [FromQuery] int? locationId = null,
     [FromQuery] DateTime? dateFrom = null,
     [FromQuery] DateTime? dateTo = null,
     CancellationToken ct = default)
    {
        var concerts = await _service.GetFilteredAsync(includeRefs, categoryId, locationId, dateFrom, dateTo, ct);
        return Ok(concerts);
    }
}
