﻿using CarHaulingAnalytics.Data.Enums;

namespace CarHaulingAnalytics.Data.Models;

public class OverviewFilterModel
{
    public int LowerPriceLimit { get; set; } = 300;
    public int UpperPriceLimit { get; set; } = 999;
    public int LowerRangeLimit { get; set; }
    public int UpperRangeLimit { get; set; } = 600;
    public int MinVehicles { get; set; } = 1;
    public int MaxVehicles { get; set; } = 10;
    public TrailerTypes? TrailerType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<States>? ExcludedStates { get; set; } = [States.AK, States.HI];
    public States? SelectedState { get; set; }
    public List<SourcePlatform> SelectedPlatforms { get; set; } = [SourcePlatform.CentralDispatch, SourcePlatform.SuperDispatch];
    public bool ExcludePickup { get; set; } = true;
    public bool ExcludeDelivery { get; set; } = true;
}
