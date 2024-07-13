namespace CarHaulingAnalytics.Data.Models;

public class AnalyzeLoadingState
{
    public bool SnapshotChartLoading { get; set; } = true;
    public bool PriceCalendarLoading { get; set; } = true;
    public bool PricePerMileCalendarLoading { get; set; } = true;
}
