using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketPricesController : ControllerBase
{
    private readonly TicketPriceService _service;

    public TicketPricesController(TicketPriceService service) => _service = service;

    [HttpGet("by-concert/{concertId:int}")]
    public async Task<IActionResult> GetByConcert(int concertId, CancellationToken ct)
        => Ok(await _service.GetByConcertAsync(concertId, ct));

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertTicketPriceRequest req, CancellationToken ct)
    {
        try
        {
            var saved = await _service.UpsertAsync(req.ConcertId, req.RegionSeatingId, req.CurrencyId, req.Amount, ct);
            return Ok(saved); 
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}