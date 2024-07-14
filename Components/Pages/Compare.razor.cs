using CarHaulingAnalytics.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen.Blazor.Rendering;
using Radzen.Blazor;
using Radzen;
using CarHaulingAnalytics.Data.Models;

namespace CarHaulingAnalytics.Components.Pages;

public class CompareRazor : LayoutComponentBase, IDisposable
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject] private WidgetDataService DataService { get; set; } = null!;

    [Inject] private NotificationService NotificationService { get; set; } = null!;

    private CancellationTokenSource CancellationTokenSource { get; set; } = new();

    protected (DateTime startDate, DateTime endDate) DatePickerDates { get; private set; }

    protected int TotalOrders { get; private set; }

    protected RadzenButton Button { get; set; } = new();
    protected Popup Popup { get; set; } = new();

    protected bool DatesLoaded { get; set; }

    protected OverviewFilterModel FilterValue { get; set; } = new()
    {
        FromDate = DateTime.UtcNow.AddDays(-7),
        ToDate = DateTime.UtcNow,
        ExcludedStates = []
    };

    public async Task FilterChanged()
    {

    }

    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }
}
