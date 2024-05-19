using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Models;
using CarHaulingAnalytics.Data.Models.Widgets;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Globalization;

namespace CarHaulingAnalytics.Components.Pages;

public class TrendsRazor : LayoutComponentBase, IDisposable
{
    [Inject] private WidgetDataService DataService { get; set; } = null!;

    private CancellationTokenSource CancellationTokenSource { get; set; } = new();

    protected IEnumerable<DateAverageTuple> AveragePriceTrends { get; private set; } = [];

    protected IEnumerable<DateAverageTuple> AveragePricePerMileTrends { get; private set; } = [];

    protected IEnumerable<DateAverageTuple> AverageDistanceTrends { get; private set; } = [];

    protected IEnumerable<int> VehicleCount = [1, 10];

    public OverviewFilterModel FilterModel { get; set; } = new()
    {
        PriceLimits = [300, 7000],
        RangeLimits = [0, 10000],
        FromDate = DateTime.UtcNow.AddDays(-7),
        ToDate = DateTime.UtcNow
    };

    protected (DateTime startDate, DateTime endDate) DatePickerDates { get; set; }

    protected bool DatePickerDisabled { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DatePickerDates = await DataService.GetLowerAndUpperDates(CancellationTokenSource.Token);
            await LoadWidgetData(FilterModel, CancellationTokenSource.Token);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadWidgetData(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        DatePickerDisabled = true;
        AveragePriceTrends = await DataService.GetAveragePriceTrend(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        AveragePricePerMileTrends = await DataService.GetAveragePricePerMileTrend(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        AverageDistanceTrends = await DataService.GetAverageDistanceTrend(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        DatePickerDisabled = false;
        await InvokeAsync(StateHasChanged);
    }

    protected async Task FilterDateValueChanged()
    {
        await LoadWidgetData(FilterModel, CancellationTokenSource.Token);
    }

    protected async Task OneWeekRange()
    {
        FilterModel.ToDate = DateTime.UtcNow;
        FilterModel.FromDate = DateTime.UtcNow.AddDays(-7);
        await LoadWidgetData(FilterModel, CancellationTokenSource.Token);
    }

    protected async Task TwoWeeksRange()
    {
        FilterModel.ToDate = DateTime.UtcNow;
        FilterModel.FromDate = DateTime.UtcNow.AddDays(-14);
        await LoadWidgetData(FilterModel, CancellationTokenSource.Token);
    }

    protected async Task OneMonthRange()
    {
        FilterModel.ToDate = DateTime.UtcNow;
        FilterModel.FromDate = DateTime.UtcNow.AddMonths(-1);
        await LoadWidgetData(FilterModel, CancellationTokenSource.Token);
    }

    protected string FormatAsUSD(object value)
    {
        return ((double)value).ToString("C2", CultureInfo.CreateSpecificCulture("en-US"));
    }

    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }
}
