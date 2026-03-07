using ConcertTickets.Reporting.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConcertTickets.Reporting;

public class ReportingDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
{
    public ReportingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReportingDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=concerttickets_reporting;Username=postgres;Password=postgres");

        return new ReportingDbContext(optionsBuilder.Options);
    }
}
