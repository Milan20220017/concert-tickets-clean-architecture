using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Domain.Models;
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
    private readonly IReservationRequestStatusRepository _requestStatuses;
    private readonly IConcertRepository _concerts;
    private readonly ITicketPriceRepository _prices;


    public ReservationsController(
        ReservationService service,
        IConnectionMultiplexer redis,
        IReservationRequestStatusRepository requestStatuses,
        IConcertRepository concerts,
        ITicketPriceRepository prices)
    {
        _service = service;
        _redis = redis;
        _requestStatuses = requestStatuses;
        _concerts = concerts;
        _prices = prices;
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
                PromoCode = req.PromoCode,
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
                loginCode
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
            var concert = await _concerts.GetByIdAsync(reservation.ConcertId, false, ct);


            var publisher = _redis.GetSubscriber();

            var reservationEvent = new ReservationEventMessage
            {
                EventType = "ReservationCancelled",
                ReservationCode = reservation.LoginCode,
                ConcertId = reservation.ConcertId,
                Email = reservation.Email,
                OccurredAt = DateTime.UtcNow,
                TicketCount = reservation.Items.Sum(i => i.Quantity),
                LocationId = concert!.LocationId


            };

            var eventJson = JsonSerializer.Serialize(reservationEvent);

            await publisher.PublishAsync(
                RedisChannel.Literal("reservation_events"),
                eventJson
            );

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

            var concert = await _concerts.GetByIdAsync(reservation.ConcertId, false, ct);


            var publisher = _redis.GetSubscriber();

            var reservationEvent = new ReservationEventMessage
            {
                EventType = "ReservationCancelled",
                ReservationCode = reservation.LoginCode,
                ConcertId = reservation.ConcertId,
                Email = reservation.Email,
                OccurredAt = DateTime.UtcNow,
                TicketCount = reservation.Items.Sum(i => i.Quantity),
                LocationId = concert!.LocationId
            };

            var eventJson = JsonSerializer.Serialize(reservationEvent);

            await publisher.PublishAsync(
                RedisChannel.Literal("reservation_events"),
                eventJson
            );

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

            var normalizedLoginCode = req.LoginCode.Trim().ToUpperInvariant();
            var normalizedEmail = req.Email.Trim();

            var requestStatus = await _requestStatuses.GetByLoginCodeAsync(normalizedLoginCode, ct);

            if (requestStatus is null)
                return NotFound(new { error = "Zahtjev za rezervaciju nije pronađen." });

            if (!string.Equals(requestStatus.Email?.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase))
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
                    ReservationStatus = null,
                    GeneratedPromoCode = null,
                    Items = new List<ReservationStatusItemDto>(),
                    SubtotalBeforeDiscounts = null,
                    EarlyBirdDiscountAmount = null,
                    PromoDiscountAmount = null,
                    FinalTotalPrice = null,
                    PriceBreakdown = new List<ReservationPriceBreakdownItemDto>()
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
                    ReservationStatus = null,
                    GeneratedPromoCode = null,
                    Items = new List<ReservationStatusItemDto>(),
                    SubtotalBeforeDiscounts = null,
                    EarlyBirdDiscountAmount = null,
                    PromoDiscountAmount = null,
                    FinalTotalPrice = null,
                    PriceBreakdown = new List<ReservationPriceBreakdownItemDto>()
                });
            }

            var reservation = await _service.GetByLoginCodeAndEmailAsync(
                requestStatus.LoginCode,
                requestStatus.Email,
                ct
            );

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
                    ReservationStatus = null,
                    GeneratedPromoCode = null,
                    Items = new List<ReservationStatusItemDto>(),
                    SubtotalBeforeDiscounts = null,
                    EarlyBirdDiscountAmount = null,
                    PromoDiscountAmount = null,
                    FinalTotalPrice = null,
                    PriceBreakdown = new List<ReservationPriceBreakdownItemDto>()
                });
            }

            var breakdown = await _service.BuildReservationBreakdownAsync(reservation, ct);

            return Ok(new CheckReservationStatusResponse
            {
                LoginCode = requestStatus.LoginCode,
                Email = requestStatus.Email,
                Status = "Accepted",
                ErrorMessage = null,
                ReservationId = reservation.Id,
                TotalPrice = reservation.TotalPrice,
                ReservationStatus = reservation.Status,
                GeneratedPromoCode = reservation.GeneratedPromoCode?.Code,
                Items = reservation.Items.Select(i => new ReservationStatusItemDto
                {
                    RegionSeatingId = i.RegionSeatingId,
                    RegionName = i.RegionSeating?.Name ?? $"Region {i.RegionSeatingId}",
                    Quantity = i.Quantity
                }).ToList(),

                SubtotalBeforeDiscounts = breakdown.SubtotalBeforeDiscounts,
                EarlyBirdDiscountAmount = breakdown.EarlyBirdDiscountAmount,
                PromoDiscountAmount = breakdown.PromoDiscountAmount,
                FinalTotalPrice = breakdown.FinalTotalPrice,
                PriceBreakdown = breakdown.PriceBreakdown
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("update-by-code")]
    public async Task<IActionResult> UpdateByCode([FromBody] UpdateReservationRequest req, CancellationToken ct)
    {
        try
        {
            var updated = await _service.UpdateReservationByCodeAsync(
                req.LoginCode,
                req.Email,
                req.Items.Select(i => (i.RegionSeatingId, i.Quantity)).ToList(),
                ct
            );

            return Ok(new
            {
                message = "Rezervacija je uspješno izmijenjena.",
                reservationId = updated.Id,
                totalPrice = updated.TotalPrice,
                status = updated.Status
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate(
     [FromBody] CalculateReservationRequest request,
     CancellationToken ct)
    {
        var result = await _service.CalculateAsync(request, ct);
        return Ok(result);
    }
}