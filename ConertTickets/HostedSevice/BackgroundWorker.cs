using ConcertTickets_API.DTO;
using ConcertTickets_API.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace ConcertTickets_API.HostedSevice
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundWorker(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
        {
            _redis = redis;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.SubscribeAsync(RedisChannel.Literal("reservation_created"), async (channel, message) =>
            {
                if (!message.HasValue)
                    return;

                Console.WriteLine($"Received message from {channel}: {message}");

                try
                {
                    var dto = JsonSerializer.Deserialize<CreateReservationMessage>(message!);

                    if (dto is null)
                    {
                        Console.WriteLine("Message deserialization failed.");
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var reservationService = scope.ServiceProvider.GetRequiredService<ReservationService>();

                    var items = dto.Items
                        .Select(i => (i.RegionSeatingId, i.Quantity))
                        .ToList();

                    var created = await reservationService.CreateAsync(
                        dto.ConcertId,
                        dto.CurrencyId,
                        dto.Email,
                        dto.UsedPromoCodeId,
                        items,
                        CancellationToken.None
                    );

                    Console.WriteLine($"Reservation created successfully. Id = {created.Id}");

                    
                    var eventPublisher = _redis.GetSubscriber();

                    var reservationEvent = new ReservationEventMessage
                    {
                        EventType = "ReservationCreated",
                        ReservationId = created.Id,
                        ConcertId = created.ConcertId,
                        Email = created.Email,
                        OccurredAt = DateTime.UtcNow
                    };

                    var eventJson = JsonSerializer.Serialize(reservationEvent);

                    await eventPublisher.PublishAsync(RedisChannel.Literal("reservation_events"), eventJson);

                    Console.WriteLine($"Published ReservationCreated event for reservation {created.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while processing reservation_created:");
                    Console.WriteLine(ex.ToString());
                }
            });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}