namespace CarHaulingAnalytics.Data.Models;

public class TrailerTypesOverviewModel
{
    public DateTime Date { get; set; }
    public int OpenCount { get; set; }
    public int EnclosedCount { get; set; }
    public int DriveawayCount { get; set; }
}

public class TrailerTypesData
{
    public string Name { get; set; } = string.Empty;
    public TrailerTypesPair[] Data { get; set; } = [];
}

public class TrailerTypesPair
{
    public long X { get; set; }
    public int Y { get; set; }
}
