using CarHaulingAnalytics.Data.Enums;

namespace CarHaulingAnalytics.Data.Models;

public class OverviewFilterModel
{
    public IEnumerable<int> PriceLimits = [300, 999];

    public IEnumerable<int> RangeLimits = [0, 600];
    public TrailerTypes? TrailerType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<States>? ExcludedStates { get; set; } = [States.AK, States.HI];
    public List<SourcePlatform> SelectedPlatforms { get; set; } = [SourcePlatform.CentralDispatch, SourcePlatform.SuperDispatch];
    public bool ExcludePickup { get; set; } = true;
    public bool ExcludeDelivery { get; set; } = true;
}
