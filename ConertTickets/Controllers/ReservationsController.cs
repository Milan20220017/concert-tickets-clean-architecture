using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _service;
    public ReservationsController(ReservationService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest req, CancellationToken ct)
    {
        try
        {
            var items = req.Items.Select(i => (i.RegionSeatingId, i.Quantity)).ToList();
            var created = await _service.CreateAsync(req.ConcertId, req.CurrencyId, req.Email, req.UsedPromoCodeId, items, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => Ok(new { note = "Dodaj GetById kad proširimo repo include-items (ako želiš)" });


    [HttpGet("by-code/{loginCode}")]
    public async Task<IActionResult> GetByCode(string loginCode, CancellationToken ct)
    {
        var res = await _service.GetByLoginCodeAsync(loginCode, ct);
        return res is null ? NotFound() : Ok(res);
    }
}
