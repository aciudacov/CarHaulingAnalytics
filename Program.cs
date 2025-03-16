using CarHaulingAnalytics.Components;
using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Context;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTransient<WidgetDataService>();

builder.Services.AddRadzenComponents();

builder.Services.AddDbContext<AnalyticContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"), options => options.EnableRetryOnFailure());
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
        

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseRouting();
app.UseAntiforgery();

await app.RunAsync();