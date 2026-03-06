using ConcertTickets.Reporting.Data;
using ConcertTickets.Reporting.HostedServices;
using ConcertTickets.Reporting.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("ReportingDb");
builder.Services.AddDbContext<ReportingDbContext>(opt => opt.UseNpgsql(cs));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddScoped<ReportingService>();
builder.Services.AddHostedService<ReservationEventsSubscriber>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();