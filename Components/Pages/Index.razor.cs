using CarHaulingAnalytics.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CarHaulingAnalytics.Data.Models;
using Radzen;
using Radzen.Blazor.Rendering;
using Radzen.Blazor;

namespace CarHaulingAnalytics.Components.Pages;

public class IndexRazor : LayoutComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject] private WidgetDataService DataService { get; set; } = null!;

    [Inject] private NotificationService NotificationService { get; set; } = null!;

    private CancellationTokenSource CancellationTokenSource { get; set; } = new();

    protected (DateTime startDate, DateTime endDate) DatePickerDates { get; private set; }

    protected int TotalOrders { get; private set; }

    protected OverviewLoadingState Loading { get; } = new();

    protected RadzenButton Button { get; set; } = new();
    protected Popup Popup { get; set; } = new();

    protected OverviewFilterModel FilterValue { get; } = new()
    {
        FromDate = DateTime.UtcNow.AddDays(-7),
        ToDate = DateTime.UtcNow
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadWidgetData(FilterValue, CancellationTokenSource.Token);
        }
        await base.OnAfterRenderAsync(firstRender);
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

    private async Task LoadWidgetData(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        DatePickerDates = await DataService.GetLowerAndUpperDates(CancellationTokenSource.Token);
        TotalOrders = await DataService.GetOrderCount(model, cancellationToken);
        await RenderHoneycombPickup(model, cancellationToken);
        await RenderHoneycombDelivery(model, cancellationToken);
        await RenderHoneycombPrice(model, cancellationToken);
        await RenderHoneycombPricePerMile(model, cancellationToken);
        await RenderShippersCloud(model, cancellationToken);
        await RenderDependencyPickup(model, cancellationToken);
        await RenderPaymentTypes(model, cancellationToken);
        await RenderVehicleStatus(model, cancellationToken);
        await RenderTrailerType(model, cancellationToken);
        await RenderVehicleCount(model, cancellationToken);
    }

    private async Task RenderHoneycombPickup(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.PickupCountLoading = true;
        await InvokeAsync(StateHasChanged);
        var pickupData = await DataService.GetCountByPickupState(model, cancellationToken);
        Loading.PickupCountLoading = false;
        await InvokeAsync(StateHasChanged);
        var pickupDataDictionary = pickupData.ToDictionary(k => k.Value, e => e.Count);
        var minValue = pickupDataDictionary.Values.Min();
        var maxValue = pickupDataDictionary.Values.Max();
        var rangeSpan = (maxValue - minValue) / 4;
        var ranges = new[]
        {
            new { from = minValue, to = minValue + rangeSpan, color = "#F9EDB3", name = "< 1/4 Max" },
            new { from = minValue + rangeSpan, to = minValue + 2 * rangeSpan, color = "#FFC428", name = "1/4 - 1/2 Max" },
            new { from = minValue + 2 * rangeSpan, to = minValue + 3 * rangeSpan, color = "#FF7987", name = "1/2 - 3/4 Max" },
            new { from = minValue + 3 * rangeSpan, to = maxValue, color = "#FF2371", name = "> 3/4 Max" }
        };
        await JsRuntime.InvokeVoidAsync("renderHoneycombMap", cancellationToken, "honeycombPickup", ranges, pickupDataDictionary, "Pickup order count");
    }

    private async Task RenderHoneycombDelivery(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.DeliveryCountLoading = true;
        await InvokeAsync(StateHasChanged);
        var deliveryDate = await DataService.GetCountByDeliveryState(model, cancellationToken);
        Loading.DeliveryCountLoading = false;
        await InvokeAsync(StateHasChanged);
        var deliveryDataDictionary = deliveryDate.ToDictionary(k => k.Value, e => e.Count);
        var minValue = deliveryDataDictionary.Values.Min();
        var maxValue = deliveryDataDictionary.Values.Max();
        var rangeSpan = (maxValue - minValue) / 4;
        var ranges = new[]
        {
            new { from = minValue, to = minValue + rangeSpan, color = "#F9EDB3", name = "< 1/4 Max" },
            new { from = minValue + rangeSpan, to = minValue + 2 * rangeSpan, color = "#FFC428", name = "1/4 - 1/2 Max" },
            new { from = minValue + 2 * rangeSpan, to = minValue + 3 * rangeSpan, color = "#FF7987", name = "1/2 - 3/4 Max" },
            new { from = minValue + 3 * rangeSpan, to = maxValue, color = "#FF2371", name = "> 3/4 Max" }
        };
        await JsRuntime.InvokeVoidAsync("renderHoneycombMap", cancellationToken, "honeycombDelivery", ranges, deliveryDataDictionary, "Delivery order count");
    }

    private async Task RenderHoneycombPrice(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.AveragePriceLoading = true;
        await InvokeAsync(StateHasChanged);
        var pickupData = await DataService.GetAveragePriceByPickupState(model, cancellationToken);
        Loading.AveragePriceLoading = false;
        await InvokeAsync(StateHasChanged);
        var pickupDataDictionary = pickupData.ToDictionary(k => k.State, e => e.AveragePrice);
        var minValue = pickupDataDictionary.Values.Min();
        var maxValue = pickupDataDictionary.Values.Max();
        var rangeSpan = (maxValue - minValue) / 4;
        var ranges = new[]
        {
            new { from = minValue, to = minValue + rangeSpan, color = "#FF1100", name = "< 1/4 Max" },
            new { from = minValue + rangeSpan, to = minValue + 2 * rangeSpan, color = "#FF7700", name = "1/4 - 1/2 Max" },
            new { from = minValue + 2 * rangeSpan, to = minValue + 3 * rangeSpan, color = "#FFDD00", name = "1/2 - 3/4 Max" },
            new { from = minValue + 3 * rangeSpan, to = maxValue, color = "#AEFF00", name = "> 3/4 Max" }
        };
        await JsRuntime.InvokeVoidAsync("renderHoneycombMap", cancellationToken, "honeycombPrice", ranges, pickupDataDictionary, "Average order price");
    }

    private async Task RenderHoneycombPricePerMile(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.AveragePricePerMileLoading = true;
        await InvokeAsync(StateHasChanged);
        var deliveryDate = await DataService.GetAveragePricePerMileByPickupState(model, cancellationToken);
        Loading.AveragePricePerMileLoading = false;
        await InvokeAsync(StateHasChanged);
        var deliveryDataDictionary = deliveryDate.ToDictionary(k => k.State, e => e.AveragePrice);
        var minValue = deliveryDataDictionary.Values.Min();
        var maxValue = deliveryDataDictionary.Values.Max();
        var rangeSpan = (maxValue - minValue) / 4;
        var ranges = new[]
        {
            new { from = minValue, to = minValue + rangeSpan, color = "#FF1100", name = "< 1/4 Max" },
            new { from = minValue + rangeSpan, to = minValue + 2 * rangeSpan, color = "#FF7700", name = "1/4 - 1/2 Max" },
            new { from = minValue + 2 * rangeSpan, to = minValue + 3 * rangeSpan, color = "#FFDD00", name = "1/2 - 3/4 Max" },
            new { from = minValue + 3 * rangeSpan, to = maxValue, color = "#AEFF00", name = "> 3/4 Max" }
        };
        await JsRuntime.InvokeVoidAsync("renderHoneycombMap", cancellationToken, "honeycombPricePerMile", ranges, deliveryDataDictionary, "Average price per mile");
    }

    private async Task RenderDependencyPickup(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.PopularRoutesLoading = true;
        await InvokeAsync(StateHasChanged);
        var popularRoutes = await DataService.GetPopularRoutes(model, cancellationToken);
        Loading.PopularRoutesLoading = false;
        await InvokeAsync(StateHasChanged);
        var routesDictionary = popularRoutes.ToDictionary(r => r.Value, d => d.Count);
        await JsRuntime.InvokeVoidAsync("renderDependencyChart", cancellationToken, "dependencyPickup", routesDictionary);
    }

    private async Task RenderShippersCloud(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.PopularShippersLoading = true;
        await InvokeAsync(StateHasChanged);
        var popularShippers = await DataService.GetShipperOrderCount(model, cancellationToken);
        Loading.PopularShippersLoading = false;
        await InvokeAsync(StateHasChanged);
        var shipperArray = popularShippers.Select(s => new
        {
            name = s.Value,
            weight = s.Count
        }).ToArray();
        await JsRuntime.InvokeVoidAsync("renderWordCloud", cancellationToken, "shipperCloud", shipperArray);
    }

    private async Task RenderPaymentTypes(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.PaymentTypesLoading = true;
        await InvokeAsync(StateHasChanged);
        var paymentTypes = await DataService.GetPaymentTypesCount(model, cancellationToken);
        Loading.PaymentTypesLoading = false;
        await InvokeAsync(StateHasChanged);
        var paymentArray = paymentTypes.Select(p => new
        {
            name = p.Value,
            y = p.Count
        }).ToArray();
        await JsRuntime.InvokeVoidAsync("renderDonutChart", cancellationToken, "paymentBreakdown", paymentArray,
            "Orders by payment type");
    }

    private async Task RenderVehicleStatus(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.VehicleStatusesLoading = true;
        await InvokeAsync(StateHasChanged);
        var paymentTypes = await DataService.GetVehicleStatuses(model, cancellationToken);
        Loading.VehicleStatusesLoading = false;
        await InvokeAsync(StateHasChanged);
        var paymentArray = paymentTypes.Select(p => new
        {
            name = p.Value,
            y = p.Count
        }).ToArray();
        await JsRuntime.InvokeVoidAsync("renderDonutChart", cancellationToken, "vehicleStatusBreakdown", paymentArray,
            "Orders by vehicle status");
    }

    private async Task RenderTrailerType(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.TrailerTypesLoading = true;
        await InvokeAsync(StateHasChanged);
        var paymentTypes = await DataService.GetTrailerTypes(model, cancellationToken);
        Loading.TrailerTypesLoading = false;
        await InvokeAsync(StateHasChanged);
        var paymentArray = paymentTypes.Select(p => new
        {
            name = p.Value,
            y = p.Count
        }).ToArray();
        await JsRuntime.InvokeVoidAsync("renderDonutChart", cancellationToken, "trailerTypeBreakdown", paymentArray,
            "Orders by trailer type");
    }

    private async Task RenderVehicleCount(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        Loading.VehicleCountLoading = true;
        await InvokeAsync(StateHasChanged);
        var paymentTypes = await DataService.GetVehicleCounts(model, cancellationToken);
        Loading.VehicleCountLoading = false;
        await InvokeAsync(StateHasChanged);
        var paymentArray = paymentTypes.Select(p => new
        {
            name = p.Value,
            y = p.Count
        }).ToArray();
        await JsRuntime.InvokeVoidAsync("renderDonutChart", cancellationToken, "vehicleCountBreakdown", paymentArray,
            "Orders by vehicle count");
    }

    public async ValueTask DisposeAsync()
    {
        await JsRuntime.InvokeVoidAsync("disposeCharts");
        await CancellationTokenSource.CancelAsync();
        CancellationTokenSource.Dispose();
        Button.Dispose();
        Popup.Dispose();
    }
}
