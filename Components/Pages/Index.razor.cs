using CarHaulingAnalytics.Data.Models.Widgets;
using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Globalization;

namespace CarHaulingAnalytics.Components.Pages;

public class IndexRazor : LayoutComponentBase, IDisposable
{
    [Inject] private WidgetDataService DataService { get; set; } = null!;

    private CancellationTokenSource CancellationTokenSource { get; set; } = new();

    protected IEnumerable<StringCountTuple> PickupOrdersCount { get; private set; } = [];

    protected IEnumerable<StringCountTuple> DeliveryOrdersCount { get; private set; } = [];

    protected IEnumerable<PickupStateAveragePrice> AveragePrices { get; private set; } = [];

    protected IEnumerable<PickupStateAveragePrice> AveragePricesPerMile { get; private set; } = [];

    protected IEnumerable<StringCountTuple> PaymentTypesCount { get; private set; } = [];

    protected IEnumerable<StringCountTuple> PopularRoutes { get; private set; } = [];

    protected IEnumerable<StringCountTuple> VehicleStatus { get; private set; } = [];

    protected IEnumerable<StringCountTuple> TrailerTypes { get; private set; } = [];

    protected IEnumerable<StringCountTuple> ShipperOrders { get; private set; } = [];

    protected int TotalOrders { get; private set; }

    protected bool DatePickerDisabled { get; private set; }

    protected (DateTime startDate, DateTime endDate) DatePickerDates { get; private set; }

    protected OverviewFilterModel FilterValue { get; set; } = new OverviewFilterModel
    {
        FromDate = DateTime.UtcNow.AddDays(-7),
        ToDate = DateTime.UtcNow
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DatePickerDates = await DataService.GetLowerAndUpperDates(CancellationTokenSource.Token);
            await LoadWidgetData(FilterValue, CancellationTokenSource.Token);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadWidgetData(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        TotalOrders = await DataService.GetOrderCount(model, cancellationToken);
        DatePickerDates = await DataService.GetLowerAndUpperDates(CancellationTokenSource.Token);
        PickupOrdersCount = await DataService.GetCountByPickupState(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        DeliveryOrdersCount = await DataService.GetCountByDeliveryState(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        PopularRoutes = await DataService.GetPopularRoutes(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        AveragePrices = await DataService.GetAveragePriceByPickupState(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        AveragePricesPerMile = await DataService.GetAveragePricePerMileByPickupState(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        ShipperOrders = await DataService.GetShipperOrderCount(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        PaymentTypesCount = await DataService.GetPaymentTypesCount(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        VehicleStatus = await DataService.GetVehicleStatuses(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
        TrailerTypes = await DataService.GetTrailerTypes(model, cancellationToken);
        await InvokeAsync(StateHasChanged);
    }

    private void ClearData()
    {
        PickupOrdersCount = [];
        DeliveryOrdersCount = [];
        AveragePrices = [];
        AveragePricesPerMile = [];
        PaymentTypesCount = [];
        PopularRoutes = [];
        ShipperOrders = [];
        VehicleStatus = [];
        TrailerTypes = [];
    }

    protected async Task FilterChanged()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
        await Task.Delay(1000);
        ClearData();
        await InvokeAsync(StateHasChanged);
        CancellationTokenSource = new();
        await LoadWidgetData(FilterValue, CancellationTokenSource.Token);
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
