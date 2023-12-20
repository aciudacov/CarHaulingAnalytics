using CarHaulingAnalytics.Data.Enums;

namespace CarHaulingAnalytics.Data.Models;

public class OverviewFilterModel
{
    public IEnumerable<int> PriceLimits = new[] { 300, 999 };

    public IEnumerable<int> RangeLimits = new[] { 0, 600 };
    public TrailerTypes? TrailerType { get; set; }
    public DateTime? Date { get; set; }
    public List<States>? ExcludedStates { get; set; } = new() { States.AK, States.HI };
    public bool ExcludePickup { get; set; }
    public bool ExcludeDelivery { get; set; }
}
