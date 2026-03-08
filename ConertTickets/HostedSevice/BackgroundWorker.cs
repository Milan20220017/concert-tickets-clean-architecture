using ConcertTickets_API.DataAccess.Repositories;
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

            await subscriber.SubscribeAsync(
                RedisChannel.Literal("reservation_created"),
                async (channel, message) =>
                {
                    CreateReservationMessage? dto = null;

                    try
                    {
                        if (!message.HasValue)
                            return;

                        Console.WriteLine($"Received message from {channel}: {message}");

                        dto = JsonSerializer.Deserialize<CreateReservationMessage>(message!);

                        if (dto is null)
                        {
                            Console.WriteLine("Message deserialization failed.");
                            return;
                        }

                        using var scope = _scopeFactory.CreateScope();

                        var reservationService = scope.ServiceProvider.GetRequiredService<ReservationService>();
                        var requestStatusRepository = scope.ServiceProvider.GetRequiredService<IReservationRequestStatusRepository>();

                        var requestStatus = await requestStatusRepository.GetByLoginCodeAsync(dto.LoginCode, CancellationToken.None);

                        if (requestStatus is null)
                        {
                            Console.WriteLine($"ReservationRequestStatus not found for loginCode {dto.LoginCode}");
                            return;
                        }

                        var items = dto.Items
                            .Select(i => (i.RegionSeatingId, i.Quantity))
                            .ToList();

                        var created = await reservationService.CreateAsync(
                            dto.LoginCode,
                            dto.ConcertId,
                            dto.CurrencyId,
                            dto.Email,
                            items,
                            dto.UsedPromoCodeId,
                            CancellationToken.None
                        );

                        requestStatus.Status = "Accepted";
                        requestStatus.ErrorMessage = null;
                        requestStatus.UpdatedAt = DateTime.UtcNow;

                        await requestStatusRepository.SaveAsync(CancellationToken.None);

                        Console.WriteLine($"Reservation created successfully. Id = {created.Id}");

                        var eventPublisher = _redis.GetSubscriber();

                        var reservationEvent = new ReservationEventMessage
                        {
                            EventType = "ReservationCreated",
                            ReservationCode = created.LoginCode,
                            ConcertId = created.ConcertId,
                            Email = created.Email,
                            OccurredAt = DateTime.UtcNow,
                            TicketCount = created.Items.Sum(i => i.Quantity)
                        };

                        var eventJson = JsonSerializer.Serialize(reservationEvent);

                        await eventPublisher.PublishAsync(
                            RedisChannel.Literal("reservation_events"),
                            eventJson
                        );

                        Console.WriteLine($"Published ReservationCreated event for reservation {created.Id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while processing reservation_created:");
                        Console.WriteLine(ex.ToString());

                        if (dto is not null)
                        {
                            try
                            {
                                using var scope = _scopeFactory.CreateScope();
                                var requestStatusRepository = scope.ServiceProvider.GetRequiredService<IReservationRequestStatusRepository>();

                                var requestStatus = await requestStatusRepository.GetByLoginCodeAsync(dto.LoginCode, CancellationToken.None);

                                if (requestStatus is not null)
                                {
                                    requestStatus.Status = "Rejected";
                                    requestStatus.ErrorMessage = ex.Message;
                                    requestStatus.UpdatedAt = DateTime.UtcNow;

                                    await requestStatusRepository.SaveAsync(CancellationToken.None);
                                }
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine("Failed to update ReservationRequestStatus after worker error:");
                                Console.WriteLine(innerEx.ToString());
                            }
                        }
                    }
                });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}