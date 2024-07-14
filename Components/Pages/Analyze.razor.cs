using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor.Rendering;
using Radzen.Blazor;
using System.Globalization;

namespace CarHaulingAnalytics.Components.Pages;

public class AnalyzeRazor : LayoutComponentBase, IAsyncDisposable
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

    private AnalyzeLoadingState Loading { get; set; } = new();

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
        await RenderOperabilityOverview(model, cancellationToken);
        await RenderTrailerTypesOverview(model, cancellationToken);
        await RenderPaymentTypesOverview(model, cancellationToken);
        await RenderVehicleCountOverview(model, cancellationToken);
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
        await JsRuntime.InvokeVoidAsync("renderCalendarChart", cancellationToken, "pricePerMileCalendar", pricesCalendar, "Average prices per mile by day");
    }

    private async Task RenderOperabilityOverview(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var operabilityTrend = await DataService.GetOperabilityTrend(model, cancellationToken);
        var operableArray = new OperabilityData
        {
            Name = "Operable",
            Data = operabilityTrend.Select(o => new OperabilityPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.OperableCount
            }).ToArray()
        };
        var inoperableArray = new OperabilityData
        {
            Name = "Inoperable",
            Data = operabilityTrend.Select(o => new OperabilityPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.InoperableCount
            }).ToArray()
        };
        var chartData = new[]
        {
            operableArray, inoperableArray
        };
        await JsRuntime.InvokeVoidAsync("renderComparisonChart", cancellationToken, "operabilityArea", "Operability trend", chartData);
    }

    private async Task RenderTrailerTypesOverview(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var trailersTrend = await DataService.GetTrailersTrend(model, cancellationToken);
        var openArray = new TrailerTypesData
        {
            Name = "Open",
            Data = trailersTrend.Select(o => new TrailerTypesPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.OpenCount
            }).ToArray()
        };
        var enclosedArray = new TrailerTypesData
        {
            Name = "Enclosed",
            Data = trailersTrend.Select(o => new TrailerTypesPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.EnclosedCount
            }).ToArray()
        };
        var driveawayArray = new TrailerTypesData
        {
            Name = "Enclosed",
            Data = trailersTrend.Select(o => new TrailerTypesPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.DriveawayCount
            }).ToArray()
        };
        var chartData = new[]
        {
            openArray, enclosedArray, driveawayArray
        };
        await JsRuntime.InvokeVoidAsync("renderComparisonChart", cancellationToken, "trailerTypeArea", "Trailer types trend", chartData);
    }

    private async Task RenderPaymentTypesOverview(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var paymentsTrend = await DataService.GetPaymentsTrend(model, cancellationToken);
        var cashArray = new PaymentTypeData
        {
            Name = "Cash",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.CashCount
            }).ToArray()
        };
        var checkArray = new PaymentTypeData
        {
            Name = "Check",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.CheckCount
            }).ToArray()
        };
        var companyCheckArray = new PaymentTypeData
        {
            Name = "Company check",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.CompanyCheckCount
            }).ToArray()
        };
        var comCheckArray = new PaymentTypeData
        {
            Name = "Comchek",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.ComcheckCount
            }).ToArray()
        };
        var tchArray = new PaymentTypeData
        {
            Name = "TCH",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.TCHCount
            }).ToArray()
        };
        var achArray = new PaymentTypeData
        {
            Name = "ACH",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.ACHCount
            }).ToArray()
        };
        var superPayArray = new PaymentTypeData
        {
            Name = "SuperPay",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.SuperPayCount
            }).ToArray()
        };
        var zelleArray = new PaymentTypeData
        {
            Name = "Zelle",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.ZelleCount
            }).ToArray()
        };
        var ushipArray = new PaymentTypeData
        {
            Name = "Uship",
            Data = paymentsTrend.Select(o => new PaymentTypePair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.UshipCount
            }).ToArray()
        };
        var chartData = new[]
        {
            cashArray, checkArray, companyCheckArray, comCheckArray, tchArray, achArray, superPayArray, zelleArray, ushipArray
        };
        await JsRuntime.InvokeVoidAsync("renderComparisonChart", cancellationToken, "paymentsTrend", "Payment types trend", chartData);
    }

    private async Task RenderVehicleCountOverview(OverviewFilterModel model, CancellationToken cancellationToken)
    {
        var paymentsTrend = await DataService.GetVehicleCountTrend(model, cancellationToken);
        var oneArray = new VehicleCountData
        {
            Name = "1 vehicle",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.One
            }).ToArray()
        };
        var twoArray = new VehicleCountData
        {
            Name = "2 vehicles",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.Two
            }).ToArray()
        };
        var threeArray = new VehicleCountData
        {
            Name = "3 vehicles",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.Three
            }).ToArray()
        };
        var fourArray = new VehicleCountData
        {
            Name = "4 vehicles",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.Four
            }).ToArray()
        };
        var fiveArray = new VehicleCountData
        {
            Name = "5 vehicles",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.Five
            }).ToArray()
        };
        var sixArray = new VehicleCountData
        {
            Name = "6 vehicles",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.Six
            }).ToArray()
        };
        var sevenArray = new VehicleCountData
        {
            Name = "7 vehicles",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.Seven
            }).ToArray()
        };
        var eightArray = new VehicleCountData
        {
            Name = "8 vehicles",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.Eight
            }).ToArray()
        };
        var nineArray = new VehicleCountData
        {
            Name = "9 vehicles",
            Data = paymentsTrend.Select(o => new VehicleCountPair
            {
                X = ((DateTimeOffset)o.Date).ToUnixTimeMilliseconds(),
                Y = o.Nine
            }).ToArray()
        };
        var chartData = new[]
        {
            oneArray, twoArray, threeArray, fourArray, fiveArray, sixArray, sevenArray, eightArray, nineArray
        };
        await JsRuntime.InvokeVoidAsync("renderComparisonChart", cancellationToken, "vehicleCountTrend", "Vehicle count trend", chartData);
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

    public async ValueTask DisposeAsync()
    {
        await JsRuntime.InvokeVoidAsync("disposeCharts");
        await CancellationTokenSource.CancelAsync();
        CancellationTokenSource.Dispose();
        Button.Dispose();
        Popup.Dispose();
    }
}
