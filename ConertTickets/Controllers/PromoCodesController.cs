using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromoCodesController : ControllerBase
{
    private readonly PromoCodeService _service;

    public PromoCodesController(PromoCodeService service)
    {
        _service = service;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidatePromoCodeRequest req, CancellationToken ct)
    {
        var result = await _service.ValidateAsync(req.Code, ct);
        return Ok(result);
    }
}