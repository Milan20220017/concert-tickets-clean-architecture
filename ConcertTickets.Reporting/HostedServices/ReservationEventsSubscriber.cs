using ConcertTickets.Reporting.Data;
using ConcertTickets.Reporting.DTO;
using ConcertTickets.Reporting.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace ConcertTickets.Reporting.HostedServices;

public class ReservationEventsSubscriber : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReservationEventsSubscriber(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal("reservation_events"), async (channel, message) =>
        {
            if (!message.HasValue)
                return;

            try
            {
                var evt = JsonSerializer.Deserialize<ReservationEventMessage>(message!);

                if (evt is null)
                {
                    Console.WriteLine("A.2 subscriber: deserialization failed.");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();

                var exists = await db.ReservationEventLogs
                    .AnyAsync(x =>
                        x.EventType == evt.EventType &&
                        x.ReservationId == evt.ReservationId &&
                        x.OccurredAt == evt.OccurredAt,
                        stoppingToken);

                if (exists)
                {
                    Console.WriteLine($"A.2 subscriber: duplicate ignored for reservation {evt.ReservationId}");
                    return;
                }

                db.ReservationEventLogs.Add(new ReservationEventLog
                {
                    EventType = evt.EventType,
                    ReservationId = evt.ReservationId,
                    ConcertId = evt.ConcertId,
                    Email = evt.Email,
                    OccurredAt = evt.OccurredAt,
                    TicketCount = evt.TicketCount
                });

                await db.SaveChangesAsync(stoppingToken);

                Console.WriteLine($"A.2 subscriber saved event: {evt.EventType} / ReservationId={evt.ReservationId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("A.2 subscriber error:");
                Console.WriteLine(ex.ToString());
            }
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}