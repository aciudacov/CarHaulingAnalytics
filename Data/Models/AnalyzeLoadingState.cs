namespace CarHaulingAnalytics.Data.Models;

public class AnalyzeLoadingState
{
    public bool PricePerMileSeriesLoading { get; set; } = true;
    public bool PriceSeriesLoading { get; set; } = true;
    public bool PriceCalendarLoading { get; set; } = true;
    public bool PricePerMileCalendarLoading { get; set; } = true;
}
