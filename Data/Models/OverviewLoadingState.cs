namespace CarHaulingAnalytics.Data.Models;

public class OverviewLoadingState
{
    public bool PickupCountLoading { get; set; } = true;
    public bool DeliveryCountLoading { get; set; } = true;
    public bool AveragePriceLoading { get; set; } = true;
    public bool AveragePricePerMileLoading { get; set; } = true;
    public bool PopularShippersLoading { get; set; } = true;
    public bool PopularRoutesLoading { get; set; } = true;
    public bool PaymentTypesLoading { get; set; } = true;
    public bool VehicleStatusesLoading { get; set; } = true;
    public bool TrailerTypesLoading { get; set; } = true;
    public bool VehicleCountLoading { get; set; } = true;
}
