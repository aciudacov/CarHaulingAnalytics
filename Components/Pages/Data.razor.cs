using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace CarHaulingAnalytics.Components.Pages;

public class DataRazor : LayoutComponentBase
{
    [Inject] private WidgetDataService DataService { get; set; } = null!;

    protected DbSet<LoadboardOrder> Orders { get; set; }

    protected override void OnInitialized()
    {
        Orders = DataService.GetQuery();
        base.OnInitialized();
    }
}
