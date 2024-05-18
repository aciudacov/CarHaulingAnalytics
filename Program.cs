using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Context;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Serilog;

namespace CarHaulingAnalytics;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddTransient<WidgetDataService>();

        builder.Services.AddRadzenComponents();

        builder.Services.AddDbContext<AnalyticContext>(options =>
        {
            options.UseSqlServer("Server=tcp:carhaulinganalytics.database.windows.net,1433;Initial Catalog=CarHaulingAnalytics;Persist Security Info=False;User ID=carhauler;Password=cVb![0c!1K095P1X;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=90;", options => options.EnableRetryOnFailure());
            options.LogTo(Log.Information);
        }, ServiceLifetime.Transient);

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
