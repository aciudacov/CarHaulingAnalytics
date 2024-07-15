using CarHaulingAnalytics.Data.Enums;

namespace CarHaulingAnalytics.Data.Models.FilterModels;

public class CompareFilterModel : BaseFilterModel
{
    public States? This { get; set; }
    public States? That { get; set; }
    public bool Rest { get; set; }
}
