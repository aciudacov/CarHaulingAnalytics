namespace CarHaulingAnalytics.Data.Models.Charts.ComplexData;

public class OperabilityOverviewModel
{
    public DateTime Date { get; set; }
    public int OperableCount { get; set; }
    public int InoperableCount { get; set; }
}

public class OperabilityData
{
    public string Name { get; set; } = string.Empty;
    public OperabilityPair[] Data { get; set; } = [];
}

public class OperabilityPair
{
    public long X { get; set; }
    public int Y { get; set; }
}