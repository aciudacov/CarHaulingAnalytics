using CarHaulingAnalytics.Data.Models.Widgets;
using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CarHaulingAnalytics.Pages;

public class IndexRazor : LayoutComponentBase
{
    [Inject] private WidgetDataService DataService { get; set; } = null!;

    [Inject] private TooltipService TooltipService { get; set; } = null!;

    public IEnumerable<StringCountTuple> PickupOrdersCount { get; set; } = new List<StringCountTuple>();

    public IEnumerable<StringCountTuple> DeliveryOrdersCount { get; set; } = new List<StringCountTuple>();

    public IEnumerable<PickupStateAveragePrice> AveragePrices { get; set; } = new List<PickupStateAveragePrice>();

    public IEnumerable<PickupStateAveragePrice> AveragePricesPerMile { get; set; } = new List<PickupStateAveragePrice>();

    public IEnumerable<StringCountTuple> PaymentTypesCount { get; set; } = new List<StringCountTuple>();

    public IEnumerable<StringCountTuple> PopularRoutes { get; set; } = new List<StringCountTuple>();

    public IEnumerable<StringCountTuple> VehicleStatus { get; set; } = new List<StringCountTuple>();

    public IEnumerable<StringCountTuple> TrailerTypes { get; set; } = new List<StringCountTuple>();

    public IEnumerable<StringCountTuple> ShipperOrders { get; set; } = new List<StringCountTuple>();

    protected int TotalOrders { get; private set; }

    protected bool DatePickerDisabled { get; set; }

    protected (DateTime startDate, DateTime endDate) DatePickerDates { get; set; }

    protected OverviewFilterModel FilterValue { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        DatePickerDates = await DataService.GetLowerAndUpperDates();
        await LoadWidgetData(FilterValue);
        await base.OnInitializedAsync();
    }

    private async Task LoadWidgetData(OverviewFilterModel model)
    {
        DatePickerDisabled = true;
        TotalOrders = await DataService.GetOrderCount(model);
        DatePickerDates = await DataService.GetLowerAndUpperDates();
        PickupOrdersCount = await DataService.GetCountByPickupState(model);
        DeliveryOrdersCount = await DataService.GetCountByDeliveryState(model);
        AveragePrices = await DataService.GetAveragePriceByPickupState(model);
        AveragePricesPerMile = await DataService.GetAveragePricePerMileByPickupState(model);
        PaymentTypesCount = await DataService.GetPaymentTypesCount(model);
        PopularRoutes = await DataService.GetPopularRoutes(model);
        ShipperOrders = await DataService.GetShipperOrderCount(model);
        VehicleStatus = await DataService.GetVehicleStatuses(model);
        TrailerTypes = await DataService.GetTrailerTypes(model);
        DatePickerDisabled = false;
    }

    protected async Task FilterChanged()
    {
        await LoadWidgetData(FilterValue);
    }

    protected void DatePickerTooltip(ElementReference elementReference)
    {
        TooltipService.Open(elementReference, "Filter orders by date. The widgets will show data only for the date selected.", new TooltipOptions());
    }
}
