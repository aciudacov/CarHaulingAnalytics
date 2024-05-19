using CarHaulingAnalytics.Data.Models.Widgets;
using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CarHaulingAnalytics.Components.Pages;

public class IndexRazor : LayoutComponentBase
{
    [Inject] private WidgetDataService DataService { get; set; } = null!;

    [Inject] private TooltipService TooltipService { get; set; } = null!;

    protected IEnumerable<StringCountTuple> PickupOrdersCount { get; private set; } = new List<StringCountTuple>();

    protected IEnumerable<StringCountTuple> DeliveryOrdersCount { get; private set; } = new List<StringCountTuple>();

    protected IEnumerable<PickupStateAveragePrice> AveragePrices { get; private set; } = new List<PickupStateAveragePrice>();

    protected IEnumerable<PickupStateAveragePrice> AveragePricesPerMile { get; private set; } = new List<PickupStateAveragePrice>();

    protected IEnumerable<StringCountTuple> PaymentTypesCount { get; private set; } = new List<StringCountTuple>();

    protected IEnumerable<StringCountTuple> PopularRoutes { get; private set; } = new List<StringCountTuple>();

    protected IEnumerable<StringCountTuple> VehicleStatus { get; private set; } = new List<StringCountTuple>();

    protected IEnumerable<StringCountTuple> TrailerTypes { get; private set; } = new List<StringCountTuple>();

    protected IEnumerable<StringCountTuple> ShipperOrders { get; private set; } = new List<StringCountTuple>();

    protected int TotalOrders { get; private set; }

    protected bool DatePickerDisabled { get; private set; }

    protected (DateTime startDate, DateTime endDate) DatePickerDates { get; private set; }

    protected OverviewFilterModel FilterValue { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DatePickerDates = await DataService.GetLowerAndUpperDates();
            await LoadWidgetData(FilterValue);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadWidgetData(OverviewFilterModel model)
    {
        DatePickerDisabled = true;
        TotalOrders = await DataService.GetOrderCount(model);
        DatePickerDates = await DataService.GetLowerAndUpperDates();
        PickupOrdersCount = await DataService.GetCountByPickupState(model);
        await InvokeAsync(StateHasChanged);
        DeliveryOrdersCount = await DataService.GetCountByDeliveryState(model);
        await InvokeAsync(StateHasChanged);
        PopularRoutes = await DataService.GetPopularRoutes(model);
        await InvokeAsync(StateHasChanged);
        AveragePrices = await DataService.GetAveragePriceByPickupState(model);
        await InvokeAsync(StateHasChanged);
        AveragePricesPerMile = await DataService.GetAveragePricePerMileByPickupState(model);
        await InvokeAsync(StateHasChanged);
        ShipperOrders = await DataService.GetShipperOrderCount(model);
        await InvokeAsync(StateHasChanged);
        PaymentTypesCount = await DataService.GetPaymentTypesCount(model);
        await InvokeAsync(StateHasChanged);
        VehicleStatus = await DataService.GetVehicleStatuses(model);
        await InvokeAsync(StateHasChanged);
        TrailerTypes = await DataService.GetTrailerTypes(model);
        await InvokeAsync(StateHasChanged);
        DatePickerDisabled = false;
    }

    private void ClearData()
    {
        PickupOrdersCount = new List<StringCountTuple>();
        DeliveryOrdersCount = new List<StringCountTuple>();
        AveragePrices = new List<PickupStateAveragePrice>();
        AveragePricesPerMile = new List<PickupStateAveragePrice>();
        PaymentTypesCount = new List<StringCountTuple>();
        PopularRoutes = new List<StringCountTuple>();
        ShipperOrders = new List<StringCountTuple>();
        VehicleStatus = new List<StringCountTuple>();
        TrailerTypes = new List<StringCountTuple>();
    }

    protected async Task FilterChanged()
    {
        ClearData();
        await InvokeAsync(StateHasChanged);
        await LoadWidgetData(FilterValue);
    }

    protected void DatePickerTooltip(ElementReference elementReference)
    {
        TooltipService.Open(elementReference, "Filter orders by date. The widgets will show data only for the date selected.", new TooltipOptions());
    }
}
