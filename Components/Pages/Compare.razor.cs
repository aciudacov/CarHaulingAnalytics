using CarHaulingAnalytics.Data;
using CarHaulingAnalytics.Data.Models.Charts.ComplexData;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen.Blazor.Rendering;
using Radzen.Blazor;
using Radzen;
using CarHaulingAnalytics.Data.Models.FilterModels;
using EnumsNET;

namespace CarHaulingAnalytics.Components.Pages;

public class CompareRazor : LayoutComponentBase, IAsyncDisposable
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

    protected CompareFilterModel FilterValue { get; set; } = new()
    {
        FromDate = DateTime.UtcNow.AddDays(-7),
        ToDate = DateTime.UtcNow
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DatePickerDates = await DataService.GetLowerAndUpperDates(CancellationTokenSource.Token);
            DatesLoaded = true;
            await InvokeAsync(StateHasChanged);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async Task FilterChanged()
    {
        if (!FilterValue.This.HasValue)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Duration = 5000,
                Summary = "Error",
                Detail = "Please select a source state to compare"
            });
            return;
        }

        if (FilterValue is { Rest: false, That: null })
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Duration = 5000,
                Summary = "Error",
                Detail = "Please select a target state to compare or select the rest"
            });
            return;
        }
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

        await Popup.CloseAsync(Button.Element);
        await RenderOrderCountComparison(FilterValue, CancellationTokenSource.Token);
        await RenderPriceComparison(FilterValue, CancellationTokenSource.Token);
        await RenderPricePerMileComparison(FilterValue, CancellationTokenSource.Token);
        await RenderRangeComparison(FilterValue, CancellationTokenSource.Token);
    }

    private async Task RenderOrderCountComparison(CompareFilterModel model, CancellationToken cancellationToken)
    {
        var (set1, set2) = await DataService.GetOrderCountComparison(model, cancellationToken);
        var dataset1 = new OperabilityData
        {
            Name = model.This!.Value.AsString(),
            Data = set1.Select(s => new OperabilityPair
            {
                X = s.Timestamp,
                Y = s.Count
            }).ToArray()
        };
        var dataset2 = new OperabilityData
        {
            Name = model.Rest ? "other states" : model.That!.Value.AsString(),
            Data = set2.Select(s => new OperabilityPair
            {
                X = s.Timestamp,
                Y = s.Count
            }).ToArray()
        };
        var chartData = new []{dataset1, dataset2};

        await JsRuntime.InvokeVoidAsync("renderComparisonChart", cancellationToken, "compareCount", chartData, "Order count comparison", 0, "");
    }

    private async Task RenderPriceComparison(CompareFilterModel model, CancellationToken cancellationToken)
    {
        var (set1, set2) = await DataService.GetPriceComparison(model, cancellationToken);
        var dataset1 = new LongDecimalOverviewModel
        {
            Name = model.This!.Value.AsString(),
            Data = set1.Select(s => new LongDecimalModel
            {
                X = s.Timestamp,
                Y = s.Average
            }).ToArray()
        };
        var dataset2 = new LongDecimalOverviewModel
        {
            Name = model.Rest ? "other states" : model.That!.Value.AsString(),
            Data = set2.Select(s => new LongDecimalModel
            {
                X = s.Timestamp,
                Y = s.Average
            }).ToArray()
        };
        var chartData = new[] { dataset1, dataset2 };

        await JsRuntime.InvokeVoidAsync("renderComparisonChart", cancellationToken, "comparePrice", chartData, "Average price", 2, "$");
    }

    private async Task RenderPricePerMileComparison(CompareFilterModel model, CancellationToken cancellationToken)
    {
        var (set1, set2) = await DataService.GetPricePerMileComparison(model, cancellationToken);
        var dataset1 = new LongDecimalOverviewModel
        {
            Name = model.This!.Value.AsString(),
            Data = set1.Select(s => new LongDecimalModel
            {
                X = s.Timestamp,
                Y = s.Average
            }).ToArray()
        };
        var dataset2 = new LongDecimalOverviewModel
        {
            Name = model.Rest ? "other states" : model.That!.Value.AsString(),
            Data = set2.Select(s => new LongDecimalModel
            {
                X = s.Timestamp,
                Y = s.Average
            }).ToArray()
        };
        var chartData = new[] { dataset1, dataset2 };

        await JsRuntime.InvokeVoidAsync("renderComparisonChart", cancellationToken, "comparePpm", chartData, "Average price per mile", 2, "$");
    }

    private async Task RenderRangeComparison(CompareFilterModel model, CancellationToken cancellationToken)
    {
        var (set1, set2) = await DataService.GetRangeComparison(model, cancellationToken);
        var dataset1 = new LongDecimalOverviewModel
        {
            Name = model.This!.Value.AsString(),
            Data = set1.Select(s => new LongDecimalModel
            {
                X = s.Timestamp,
                Y = s.Average
            }).ToArray()
        };
        var dataset2 = new LongDecimalOverviewModel
        {
            Name = model.Rest ? "other states" : model.That!.Value.AsString(),
            Data = set2.Select(s => new LongDecimalModel
            {
                X = s.Timestamp,
                Y = s.Average
            }).ToArray()
        };
        var chartData = new[] { dataset1, dataset2 };

        await JsRuntime.InvokeVoidAsync("renderComparisonChart", cancellationToken, "compareRange", chartData, "Average range", 0, "mi");
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
