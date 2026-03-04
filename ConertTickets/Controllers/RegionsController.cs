using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/locations/{locationId:int}/regions")]
public class RegionsController : ControllerBase
{
    private readonly RegionSeatingService _service;
    public RegionsController(RegionSeatingService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(int locationId, CancellationToken ct)
        => Ok(await _service.GetByLocationAsync(locationId, ct));

    [HttpPost]
    public async Task<IActionResult> Create(int locationId, CreateRegionRequest req, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(locationId, req.Name, req.Capacity, ct);
            return Created($"/api/locations/{locationId}/regions/{created.Id}", created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}