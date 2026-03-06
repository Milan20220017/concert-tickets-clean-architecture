using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportingController : ControllerBase
{
    private readonly ReportingService _service;

    public ReportingController(ReportingService service)
    {
        _service = service;
    }

    [HttpGet("concert-sales")]
    public async Task<IActionResult> GetConcertSales(CancellationToken ct)
    {
        var data = await _service.GetConcertSalesAsync(ct);
        return Ok(data);
    }

    [HttpGet("location-sales")]
    public async Task<IActionResult> GetLocationSales(CancellationToken ct)
    {
        var data = await _service.GetLocationSalesAsync(ct);
        return Ok(data);
    }
}