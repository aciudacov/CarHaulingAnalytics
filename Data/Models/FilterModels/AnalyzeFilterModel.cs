using CarHaulingAnalytics.Data.Enums;

namespace CarHaulingAnalytics.Data.Models.FilterModels;

public class AnalyzeFilterModel : BaseFilterModel
{
    public States? SelectedState { get; set; }
}
