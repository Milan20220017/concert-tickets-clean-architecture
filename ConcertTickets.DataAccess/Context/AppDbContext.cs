using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcertTickets_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTickets_API.DataAccess.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<RegionSeating> RegionSeatings => Set<RegionSeating>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<TicketPrice> TicketPrices => Set<TicketPrice>();
        public DbSet<Concert> Concerts => Set<Concert>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<ReservationItem> ReservationItems => Set<ReservationItem>();
        public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Location>()
                .HasMany(l => l.Regions)
                .WithOne(r => r.Location!)
                .HasForeignKey(r => r.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegionSeating>()
                .HasIndex(r => new { r.LocationId, r.Name })
                .IsUnique(); //da bismo zabranili da postoje dva "VIP" regiona na istoj lokaciji

            modelBuilder.Entity<Concert>()
                .HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Concert>()
                .HasOne(c => c.Location)
                .WithMany()
                .HasForeignKey(c => c.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Currency>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<TicketPrice>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TicketPrice>()
                .HasIndex(p => new { p.ConcertId, p.RegionSeatingId, p.CurrencyId })
                .IsUnique(); // jedna cijena po (concert, region, valuta)

            modelBuilder.Entity<Reservation>()
                .Property(r => r.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Reservation>()
                .HasMany(r => r.Items)
                .WithOne(i => i.Reservation!)
                .HasForeignKey(i => i.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.UsedPromoCode)
                .WithMany()
                .HasForeignKey(r => r.UsedPromoCodeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PromoCode>()
                .HasOne(p => p.CreatedByReservation)
                .WithOne(r => r.GeneratedPromoCode)
                .HasForeignKey<PromoCode>(p => p.CreatedByReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PromoCode>()
                .HasOne(p => p.UsedByReservation)
                .WithOne()
                .HasForeignKey<PromoCode>(p => p.UsedByReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PromoCode>()
                .HasIndex(p => p.Code)
                .IsUnique();
        }
    }
}
