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
        builder.Services.AddSingleton<WeatherForecastService>();
        builder.Services.AddTransient<WidgetDataService>();

        builder.Services.AddRadzenComponents();

        builder.Services.AddDbContext<AnalyticContext>(options =>
        {
            options.UseSqlServer(builder.Configuration["DatabaseConnection"]);
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
