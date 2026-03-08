using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;
using ConcertTickets_API.Domain.Models;
namespace ConcertTickets_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _service;
    private readonly IConnectionMultiplexer _redis;
    private readonly IReservationRequestStatusRepository _requestStatuses;


    public ReservationsController(ReservationService service, IConnectionMultiplexer redis, IReservationRequestStatusRepository requestStatuses)
    {
        _service = service;
        _redis = redis;
        _requestStatuses = requestStatuses;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest req, CancellationToken ct)
    {
        try
        {
            var loginCode = Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpperInvariant();

            var requestStatus = new ReservationRequestStatus
            {
                LoginCode = loginCode,
                Email = req.Email.Trim(),
                ConcertId = req.ConcertId,
                Status = "Pending",
                ErrorMessage = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _requestStatuses.AddAsync(requestStatus, ct);

            var publisher = _redis.GetSubscriber();

            var message = new CreateReservationMessage
            {
                LoginCode = loginCode,
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

            await publisher.PublishAsync(
                RedisChannel.Literal("reservation_created"),
                json
            );

            return Accepted(new
            {
                message = "Zahtjev za rezervaciju je primljen i poslat na obradu.",
                loginCode = loginCode
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
                ReservationCode = reservation.LoginCode,
                ConcertId = reservation.ConcertId,
                Email = reservation.Email,
                OccurredAt = DateTime.UtcNow,
                TicketCount = reservation.Items.Sum(i => i.Quantity)
            };

            var eventJson = JsonSerializer.Serialize(reservationEvent);

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
                ReservationCode = reservation.LoginCode,
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

    [HttpPost("check-status")]
    public async Task<ActionResult<CheckReservationStatusResponse>> CheckStatus(
       [FromBody] CheckReservationStatusRequest req,
       CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.LoginCode) || string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { error = "Login code i email su obavezni." });

            var requestStatus = await _requestStatuses.GetByLoginCodeAsync(req.LoginCode.Trim().ToUpperInvariant(), ct);

            if (requestStatus is null)
                return NotFound(new { error = "Zahtjev za rezervaciju nije pronađen." });

            if (!string.Equals(requestStatus.Email?.Trim(), req.Email?.Trim(), StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = "Zahtjev za rezervaciju nije pronađen." });

            if (string.Equals(requestStatus.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new CheckReservationStatusResponse
                {
                    LoginCode = requestStatus.LoginCode,
                    Email = requestStatus.Email,
                    Status = "Pending",
                    ErrorMessage = null,
                    ReservationId = null,
                    TotalPrice = null,
                    ReservationStatus = null
                });
            }

            if (string.Equals(requestStatus.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new CheckReservationStatusResponse
                {
                    LoginCode = requestStatus.LoginCode,
                    Email = requestStatus.Email,
                    Status = "Rejected",
                    ErrorMessage = requestStatus.ErrorMessage,
                    ReservationId = null,
                    TotalPrice = null,
                    ReservationStatus = null
                });
            }

            var reservation = await _service.GetByLoginCodeAndEmailAsync(requestStatus.LoginCode, requestStatus.Email, ct);

            if (reservation is null)
            {
                return Ok(new CheckReservationStatusResponse
                {
                    LoginCode = requestStatus.LoginCode,
                    Email = requestStatus.Email,
                    Status = "Accepted",
                    ErrorMessage = null,
                    ReservationId = null,
                    TotalPrice = null,
                    ReservationStatus = null
                });
            }

            return Ok(new CheckReservationStatusResponse
            {
                LoginCode = requestStatus.LoginCode,
                Email = requestStatus.Email,
                Status = "Accepted",
                ErrorMessage = null,
                ReservationId = reservation.Id,
                TotalPrice = reservation.TotalPrice,
                ReservationStatus = reservation.Status
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}