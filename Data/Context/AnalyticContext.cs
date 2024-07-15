using CarHaulingAnalytics.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CarHaulingAnalytics.Data.Context;

public class AnalyticContext(DbContextOptions<AnalyticContext> options) : DbContext(options)
{
    public DbSet<LoadboardOrder> Orders { get; set; }
}
