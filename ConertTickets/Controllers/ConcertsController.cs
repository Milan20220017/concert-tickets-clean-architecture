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
    public async Task<ActionResult<ConcertListItemDto>> GetById(int id, CancellationToken ct = default)
    {
        var concert = await _service.GetListItemByIdAsync(id, ct);

        if (concert is null)
            return NotFound();

        return Ok(concert);
    }

    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllForAdmin(
    [FromQuery] int? categoryId = null,
    [FromQuery] int? locationId = null,
    [FromQuery] DateTime? dateFrom = null,
    [FromQuery] DateTime? dateTo = null,
    CancellationToken ct = default)
    {
        var concerts = await _service.GetFilteredListItemsForAdminAsync(
            categoryId,
            locationId,
            dateFrom,
            dateTo,
            ct
        );

        return Ok(concerts);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConcertRequest req, CancellationToken ct = default)
    {
        try
        {
            var created = await _service.CreateAsync(
                req.Name,
                req.Date,
                req.CategoryId,
                req.LocationId,
                req.EarlyBirdDiscountUntil,
                ct
            );
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
        try
        {
            var ok = await _service.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
        // homepage / customer list → koristi Redis cache
        if (categoryId is null && locationId is null && dateFrom is null && dateTo is null)
        {
            var concerts = await _service.GetHomeConcertsAsync(ct);
            return Ok(concerts);
        }

        var filteredConcerts = await _service.GetFilteredListItemsAsync(
            categoryId,
            locationId,
            dateFrom,
            dateTo,
            ct
        );

        return Ok(filteredConcerts);
    }
    [HttpGet("{concertId:int}/regions-availability")]
    public async Task<IActionResult> GetRegionsAvailability(int concertId, CancellationToken ct = default)
    {
        try
        {
            var result = await _service.GetRegionAvailabilityAsync(concertId, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}
