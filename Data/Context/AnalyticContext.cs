using CarHaulingAnalytics.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CarHaulingAnalytics.Data.Context;

public class AnalyticContext : DbContext
{
    public DbSet<LoadboardOrder> Orders { get; set; }

    public AnalyticContext(DbContextOptions<AnalyticContext> options) : base(options)
    {
    }
}
