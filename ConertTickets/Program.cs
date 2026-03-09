using ConcertTickets_API.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using ConcertTickets_API.DataAccess.Repositories;
using ConcertTickets_API.Services;
using System.Text.Json.Serialization;
using ConcertTickets_API.HostedSevice;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect("localhost:6379"));

// nasi servisi i repozitoriji
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<CategoryService>();

builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<LocationService>();

builder.Services.AddScoped<IRegionSeatingRepository, RegionSeatingRepository>();
builder.Services.AddScoped<RegionSeatingService>();

builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<CurrencyService>();

builder.Services.AddScoped<ITicketPriceRepository, TicketPriceRepository>();
builder.Services.AddScoped<TicketPriceService>();

builder.Services.AddScoped<IConcertRepository, ConcertRepository>();
builder.Services.AddScoped<ConcertService>();

builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<ReservationService>();

builder.Services.AddScoped<IPromoCodeRepository, PromoCodeRepository>();

builder.Services.AddScoped<IReservationRequestStatusRepository, ReservationRequestStatusRepository>();

// background service
builder.Services.AddHostedService<BackgroundWorker>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "concert-api:";
});
//spoljni api
builder.Services.AddHttpClient<ExchangeRateService>();
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(cs));
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();