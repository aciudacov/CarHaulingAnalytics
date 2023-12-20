using CarHaulingAnalytics.Data;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CarHaulingAnalytics.Pages;

public class StatesRazor : LayoutComponentBase
{
    [Inject] private WidgetDataService DataService { get; set; } = null!;

    [Inject] private TooltipService TooltipService { get; set; } = null!;

    protected DateTime? CurrentFilterDate { get; set; }

    protected (DateTime startDate, DateTime endDate) DatePickerDates { get; set; }

    protected bool DatePickerDisabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
        DatePickerDates = await DataService.GetLowerAndUpperDates();
        await base.OnInitializedAsync();
    }

    private async Task LoadWidgetData(DateTime? filterDate = null)
    {

    }

    protected void DatePickerTooltip(ElementReference elementReference)
    {
        TooltipService.Open(elementReference, "Filter orders by date. The widgets will show data only for the date selected.", new TooltipOptions());
    }

    protected async Task FilterDateValueChanged()
    {
        await LoadWidgetData(CurrentFilterDate);
    }
}
