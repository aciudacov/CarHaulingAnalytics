using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor.Rendering;
using Radzen.Blazor;
using System.Globalization;

namespace CarHaulingAnalytics.Components.Pages;

public class AnalyzeRazor : LayoutComponentBase, IDisposable
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

    protected AnalyzeLoadingState Loading { get; set; } = new();

    protected OverviewFilterModel FilterValue { get; set; } = new()
    {
        FromDate = DateTime.UtcNow.AddDays(-7),
        ToDate = DateTime.UtcNow,
        ExcludedStates = []
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (FilterValue.SelectedState.HasValue)
            {
                await LoadWidgetData(FilterValue, CancellationTokenSource.Token);
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Duration = 5000,
                    Summary = "Warning",
                    Detail = "Please select a state first"
                });
            }
            DatePickerDates = await DataService.GetLowerAndUpperDates(CancellationTokenSource.Token);
            DatesLoaded = true;
            await InvokeAsync(StateHasChanged);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadWidgetData(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        DatePickerDates = await DataService.GetLowerAndUpperDates(CancellationTokenSource.Token);
        TotalOrders = await DataService.GetOrderCount(model, cancellationToken);
        await RenderSnapshotChart(model, cancellationToken);
        await RenderPriceCalendar(model, cancellationToken);
        await RenderPricePerMileCalendar(model, cancellationToken);
    }

    private async Task RenderSnapshotChart(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var result = await DataService.GetMainTrendData(model, cancellationToken);
        await JsRuntime.InvokeVoidAsync("renderLinkedCharts", cancellationToken, "snapshotChart", result);
    }

    private async Task RenderPriceCalendar(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.PriceCalendarLoading = true;
        await InvokeAsync(StateHasChanged);
        var priceTrends = await DataService.GetAveragePriceTrend(model, cancellationToken);
        Loading.PriceCalendarLoading = false;
        await InvokeAsync(StateHasChanged);
        var pricesCalendar = priceTrends.Select(t => new
        {
            date = t.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            average = t.Average
        }).ToArray();
        await JsRuntime.InvokeVoidAsync("renderCalendarChart", cancellationToken, "priceCalendar", pricesCalendar, "Average prices by day");
    }

    private async Task RenderPricePerMileCalendar(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.PricePerMileCalendarLoading = true;
        await InvokeAsync(StateHasChanged);
        var priceTrends = await DataService.GetAveragePricePerMileTrend(model, cancellationToken);
        Loading.PricePerMileCalendarLoading = false;
        await InvokeAsync(StateHasChanged);
        var pricesCalendar = priceTrends.Select(t => new
        {
            date = t.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            average = t.Average
        }).ToArray();
        await JsRuntime.InvokeVoidAsync("renderCalendarChart", cancellationToken, "pricePerMileCalendar", pricesCalendar, "Average prices per by day");
    }

    protected async Task FilterChanged()
    {
        if (FilterValue.LowerPriceLimit > FilterValue.UpperPriceLimit)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Duration = 5000,
                Summary = "Error",
                Detail = "Minimal price cannot be greater that maximal price"
            });
            return;
        }
        if (FilterValue.LowerRangeLimit > FilterValue.UpperRangeLimit)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Duration = 5000,
                Summary = "Error",
                Detail = "Minimal range cannot be greater that maximal range"
            });
            return;
        }
        if (FilterValue.MinVehicles > FilterValue.MaxVehicles)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Duration = 5000,
                Summary = "Error",
                Detail = "Minimal vehicle amount cannot be greater that maximal amount"
            });
            return;
        }

        if (!FilterValue.SelectedState.HasValue)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Duration = 5000,
                Summary = "Warning",
                Detail = "Please select a state first"
            });
            return;
        }

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Duration = 5000,
            Summary = "Please wait",
            Detail = "Loading chart data"
        });
        await Popup.CloseAsync(Button.Element);
        await CancellationTokenSource.CancelAsync();
        CancellationTokenSource.Dispose();
        CancellationTokenSource = new CancellationTokenSource();
        await LoadWidgetData(FilterValue, CancellationTokenSource.Token);
    }

    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }
}
