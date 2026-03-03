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
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
    }
}
