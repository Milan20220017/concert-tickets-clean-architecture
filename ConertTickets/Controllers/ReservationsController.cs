using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _service;
    private readonly IConnectionMultiplexer _redis;

    public ReservationsController(ReservationService service, IConnectionMultiplexer redis)
    {
        _service = service;
        _redis = redis;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest req)
    {
        try
        {
            var publisher = _redis.GetSubscriber();

            var message = new CreateReservationMessage
            {
                ConcertId = req.ConcertId,
                CurrencyId = req.CurrencyId,
                Email = req.Email,
                UsedPromoCodeId = req.UsedPromoCodeId,
                Items = req.Items.Select(i => new CreateReservationMessageItem
                {
                    RegionSeatingId = i.RegionSeatingId,
                    Quantity = i.Quantity
                }).ToList()
            };

            var json = JsonSerializer.Serialize(message);

            // Explicitly specify the PatternMode as Literal
            await publisher.PublishAsync(new RedisChannel("reservation_created", RedisChannel.PatternMode.Literal), json);

            return Accepted(new
            {
                message = "Zahtjev za rezervaciju je primljen i poslat na obradu."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var res = await _service.GetByIdAsync(id, ct);
        return res is null ? NotFound() : Ok(res);
    }

    [HttpGet("by-code/{loginCode}")]
    public async Task<IActionResult> GetByCode(string loginCode, CancellationToken ct)
    {
        var res = await _service.GetByLoginCodeAsync(loginCode, ct);
        return res is null ? NotFound() : Ok(res);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        try
        {
            var reservation = await _service.GetByIdAsync(id, ct);
            if (reservation is null)
                return NotFound(new { error = "Rezervacija ne postoji." });

            var cancelled = await _service.CancelReservationAsync(id, ct);
            if (!cancelled)
                return NotFound(new { error = "Rezervacija ne postoji." });

            var publisher = _redis.GetSubscriber();

            var reservationEvent = new ReservationEventMessage
            {
                EventType = "ReservationCancelled",
                ReservationId = reservation.Id,
                ConcertId = reservation.ConcertId,
                Email = reservation.Email,
                OccurredAt = DateTime.UtcNow,
                TicketCount = reservation.Items.Sum(i => i.Quantity)
            };

            var eventJson = JsonSerializer.Serialize(reservationEvent);

            // Explicitly specify the PatternMode as Literal
            await publisher.PublishAsync(new RedisChannel("reservation_events", RedisChannel.PatternMode.Literal), eventJson);

            return Ok(new { message = "Rezervacija je uspješno otkazana." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    [HttpPatch("cancel-by-code")]
    public async Task<IActionResult> CancelByCode([FromBody] CancelReservationByCodeRequest req, CancellationToken ct)
    {
        try
        {
            var reservation = await _service.GetByLoginCodeAndEmailAsync(req.LoginCode, req.Email, ct);
            if (reservation is null)
                return NotFound(new { error = "Rezervacija sa datom šifrom i email adresom ne postoji." });

            var cancelled = await _service.CancelReservationAsync(reservation.Id, ct);
            if (!cancelled)
                return NotFound(new { error = "Rezervacija ne postoji." });

            var publisher = _redis.GetSubscriber();

            var reservationEvent = new ReservationEventMessage
            {
                EventType = "ReservationCancelled",
                ReservationId = reservation.Id,
                ConcertId = reservation.ConcertId,
                Email = reservation.Email,
                OccurredAt = DateTime.UtcNow,
                TicketCount = reservation.Items.Sum(i => i.Quantity)
            };

            var eventJson = JsonSerializer.Serialize(reservationEvent);
            await publisher.PublishAsync(RedisChannel.Literal("reservation_events"), eventJson);

            return Ok(new { message = "Rezervacija je uspješno otkazana." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}