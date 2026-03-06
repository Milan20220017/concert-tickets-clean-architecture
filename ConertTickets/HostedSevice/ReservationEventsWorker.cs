using ConcertTickets_API.DataAccess.Context;
using ConcertTickets_API.DTO;
using ConcertTickets_API.Domain.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace ConcertTickets_API.HostedSevice
{
    public class ReservationEventsWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceScopeFactory _scopeFactory;

        public ReservationEventsWorker(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
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
                        Console.WriteLine("reservation_events deserialization failed.");
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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

                    Console.WriteLine("=== REPORTING EVENT SAVED ===");
                    Console.WriteLine($"EventType: {evt.EventType}");
                    Console.WriteLine($"ReservationId: {evt.ReservationId}");
                    Console.WriteLine($"ConcertId: {evt.ConcertId}");
                    Console.WriteLine($"Email: {evt.Email}");
                    Console.WriteLine($"OccurredAt: {evt.OccurredAt:O}");
                    Console.WriteLine("=============================");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while processing reservation_events:");
                    Console.WriteLine(ex.ToString());
                }
            });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}