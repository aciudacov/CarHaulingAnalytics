using CarHaulingAnalytics.Data.Enums;

namespace CarHaulingAnalytics.Data.Models.FilterModels;

public class OverviewFilterModel : BaseFilterModel
{
    public List<States>? ExcludedStates { get; set; } = [States.AK, States.HI];
    public bool ExcludePickup { get; set; } = true;
    public bool ExcludeDelivery { get; set; } = true;
}
